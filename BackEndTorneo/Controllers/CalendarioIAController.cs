using BackEndTorneo.Data;
using BackEndTorneo.Models.CalendarioIA;
using BackEndTorneo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class CalendarioIAController : ControllerBase
    {
        private readonly ClaudeService _claudeService;
        private readonly TorneosData _torneosData;
        private readonly PartidosData _partidosData;
        private readonly SedesData _sedesData;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CalendarioIAController> _logger;

        public CalendarioIAController(
            ClaudeService claudeService,
            TorneosData torneosData,
            PartidosData partidosData,
            SedesData sedesData,
            IConfiguration configuration,
            ILogger<CalendarioIAController> logger)
        {
            _claudeService = claudeService;
            _torneosData = torneosData;
            _partidosData = partidosData;
            _sedesData = sedesData;
            _configuration = configuration;
            _logger = logger;
        }


        [HttpPost("generar/{torneoId}")]
        public async Task<IActionResult> GenerarCalendarioConIA(int torneoId)
        {
            try
            {
                _logger.LogInformation($"Iniciando generación de calendario con IA para torneo {torneoId}");

                var torneo = await _torneosData.ObtenerTorneo(torneoId);
                if (torneo == null)
                {
                    return NotFound(new { isSuccess = false, message = "Torneo no encontrado" });
                }

                var equipos = await _torneosData.ObtenerEquiposDelTorneo(torneoId);
                if (equipos == null || equipos.Count < 2)
                {
                    return BadRequest(new { isSuccess = false, message = "El torneo debe tener al menos 2 equipos inscritos" });
                }

                var sedes = await _sedesData.ListarSedes();
                if (sedes == null || sedes.Count == 0)
                {
                    return BadRequest(new { isSuccess = false, message = "No hay sedes disponibles para generar el calendario" });
                }

                var calendario = await _claudeService.GenerarCalendario(
                    equipos,
                    sedes.Select(s => new { s.Sede_Id, s.Sede_Nombre, s.Sede_Capacidad }).ToList<dynamic>(),
                    torneo.Torn_FechaInicio,
                    torneo.Torn_FechaFin
                );

                if (calendario == null || calendario.Partidos == null || calendario.Partidos.Count == 0)
                {
                    return StatusCode(500, new { isSuccess = false, message = "Claude no pudo generar un calendario válido" });
                }

                var erroresValidacion = ValidarPartidos(calendario.Partidos, equipos, sedes);
                if (erroresValidacion.Count > 0)
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        message = "El calendario generado tiene errores de validación",
                        errores = erroresValidacion
                    });
                }

                int partidosCreados = await GuardarPartidos(torneoId, calendario.Partidos);

                _logger.LogInformation($"Calendario generado exitosamente: {partidosCreados} partidos creados para torneo {torneoId}");

                return Ok(new
                {
                    isSuccess = true,
                    message = $"Calendario generado exitosamente con {partidosCreados} partidos",
                    data = new
                    {
                        totalPartidos = partidosCreados,
                        jornadas = calendario.Partidos.Max(p => p.Jornada),
                        explicacion = calendario.Explicacion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar calendario con IA: {ex.Message}");
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = "Error al generar calendario con IA",
                    detalle = ex.Message
                });
            }
        }


        private List<string> ValidarPartidos(List<PartidoGeneradoIA> partidos, List<dynamic> equipos, List<Models.Sedes.Sede> sedes)
        {
            var errores = new List<string>();
            var equiposIds = equipos.Select(e => (int)e.Equi_Id).ToHashSet();
            var sedesIds = sedes.Select(s => s.Sede_Id).ToHashSet();

            foreach (var partido in partidos)
            {
                if (!equiposIds.Contains(partido.EquipoLocalId))
                {
                    errores.Add($"Equipo local ID {partido.EquipoLocalId} no existe en el torneo");
                }

                if (!equiposIds.Contains(partido.EquipoVisitanteId))
                {
                    errores.Add($"Equipo visitante ID {partido.EquipoVisitanteId} no existe en el torneo");
                }

                if (partido.EquipoLocalId == partido.EquipoVisitanteId)
                {
                    errores.Add($"Un equipo no puede jugar contra sí mismo (ID: {partido.EquipoLocalId})");
                }

                if (!sedesIds.Contains(partido.SedeId))
                {
                    errores.Add($"Sede ID {partido.SedeId} no existe");
                }

                //if (partido.FechaHora < DateTime.Now)
                //{
                //    errores.Add($"La fecha del partido {partido.FechaHora} es anterior a hoy");
                //}

                if (partido.Jornada < 1)
                {
                    errores.Add($"La jornada debe ser mayor a 0");
                }
            }

            return errores;
        }


        private async Task<int> GuardarPartidos(int torneoId, List<PartidoGeneradoIA> partidos)
        {
            int partidosCreados = 0;
            string conexion = _configuration.GetConnectionString("CadenaSQL")!;

            // Lista para guardar los IDs de los árbitros disponibles
            List<int> arbitrosIds = new List<int>();

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // PASO 1: Obtener todos los usuarios con Rol_Id = 4 (Árbitros)
                SqlCommand cmdArbitros = new SqlCommand("SELECT Usua_Id FROM Usuarios WHERE Rol_Id = 4", con);
                using (var reader = await cmdArbitros.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        arbitrosIds.Add(reader.GetInt32(0));
                    }
                }

                // PASO 2: Guardar partidos asignando árbitros en orden (Round Robin)
                int arbitroIndex = 0;

                foreach (var partido in partidos)
                {
                    object arbitroIdParaInsertar = DBNull.Value;

                    // Si hay árbitros disponibles, asignamos uno y rotamos al siguiente
                    if (arbitrosIds.Count > 0)
                    {
                        arbitroIdParaInsertar = arbitrosIds[arbitroIndex];
                        arbitroIndex = (arbitroIndex + 1) % arbitrosIds.Count;
                    }

                    SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Partidos (
                    Torn_Id,
                    Equi_Id_Local,
                    Equi_Id_Visitante,
                    Part_FechaPartido,
                    Sede_Id,
                    Part_GolesLocal,
                    Part_GolesVisitante,
                    Part_Estado,
                    Part_Jornada,
                    Part_FechaCreacion,
                    Usua_Id
                )
                VALUES (
                    @TorneoId,
                    @EquipoLocal,
                    @EquipoVisitante,
                    @FechaPartido,
                    @SedeId,
                    0,
                    0,
                    'Programado',
                    @Jornada,
                    GETDATE(),
                    @UsuaId
                )", con);

                    cmd.Parameters.AddWithValue("@TorneoId", torneoId);
                    cmd.Parameters.AddWithValue("@EquipoLocal", partido.EquipoLocalId);
                    cmd.Parameters.AddWithValue("@EquipoVisitante", partido.EquipoVisitanteId);
                    cmd.Parameters.AddWithValue("@FechaPartido", partido.FechaHora);
                    cmd.Parameters.AddWithValue("@SedeId", partido.SedeId);
                    cmd.Parameters.AddWithValue("@Jornada", partido.Jornada);
                    cmd.Parameters.AddWithValue("@UsuaId", arbitroIdParaInsertar);

                    await cmd.ExecuteNonQueryAsync();
                    partidosCreados++;
                }
            }

            return partidosCreados;
        }

        [HttpGet("costo-estimado/{torneoId}")]
        public async Task<IActionResult> ObtenerCostoEstimado(int torneoId)
        {
            try
            {
                
                bool yaTienePartidos = false;
                string conexion = _configuration.GetConnectionString("CadenaSQL")!;
                using (var con = new SqlConnection(conexion))
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Partidos WHERE Torn_Id = @TornId", con);
                    cmd.Parameters.AddWithValue("@TornId", torneoId);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    yaTienePartidos = count > 0;
                }

                var equipos = await _torneosData.ObtenerEquiposDelTorneo(torneoId);

                // Si no tiene partidos, validamos que haya equipos suficientes
                if (!yaTienePartidos && (equipos == null || equipos.Count < 2))
                {
                    return BadRequest(new { isSuccess = false, message = "El torneo debe tener al menos 2 equipos inscritos" });
                }

                int numeroEquipos = equipos?.Count ?? 0;
                int partidosEstimados = numeroEquipos * (numeroEquipos - 1);

                int tokensInputEstimados = 1000 + (numeroEquipos * 50);
                int tokensOutputEstimados = partidosEstimados * 100;

                double costoInput = (tokensInputEstimados / 1000000.0) * 1.0;
                double costoOutput = (tokensOutputEstimados / 1000000.0) * 5.0;
                double costoTotal = costoInput + costoOutput;

                return Ok(new
                {
                    isSuccess = true,
                    data = new
                    {
                        yaTienePartidos, // <--- Esta bandera le dice al front qué botones mostrar
                        numeroEquipos,
                        partidosEstimados,
                        tokensInputEstimados,
                        tokensOutputEstimados,
                        costoEstimadoUSD = Math.Round(costoTotal, 4),
                        modelo = "Claude Haiku 4.5"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = ex.Message });
            }
        }
    }
}