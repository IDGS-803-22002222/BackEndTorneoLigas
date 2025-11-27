using BackEndTorneo.Data;
using BackEndTorneo.Models.Estadisticas;
using BackEndTorneo.Models.Jugadores;
using BackEndTorneo.Models.Partidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/partido")]
    [ApiController]
    [Authorize]
    public class PartidoAppController : ControllerBase
    {
        private readonly PartidosData _partidosData;
        private readonly JugadoresData _jugadoresData;
        private readonly EstadisticasData _estadisticasData;

        public PartidoAppController(PartidosData partidosData, JugadoresData jugadoresData, EstadisticasData estadisticasData)
        {
            _partidosData = partidosData;
            _jugadoresData = jugadoresData;
            _estadisticasData = estadisticasData;
        }

        // DTOs alineados con los modelos de Android
        public class EquipoDto
        {
            public int equipoID { get; set; }
            public string nombreEquipo { get; set; } = string.Empty;
        }

        public class PartidoDto
        {
            public int partidoID { get; set; }
            public int equipoLocalID { get; set; }
            public int equipoVisitanteID { get; set; }
            public string fechaPartido { get; set; } = string.Empty;
            public string lugarPartido { get; set; } = string.Empty;
            public int? golesLocal { get; set; }
            public int? golesVisitante { get; set; }
            public string estado { get; set; } = string.Empty;
            public int? arbitroID { get; set; }
            public EquipoDto? equipoLocal { get; set; }
            public EquipoDto? equipoVisitante { get; set; }
        }

        public class EventoPartidoDto
        {
            public int? eventoID { get; set; }
            public int partidoID { get; set; }
            public int jugadorID { get; set; }
            public string tipoEvento { get; set; } = string.Empty; // Gol, Asistencia, TarjetaAmarilla, TarjetaRoja
            public int minuto { get; set; }
            public string? descripcion { get; set; }
        }

        public class RegistrarEventoRequest
        {
            public int partidoID { get; set; }
            public int jugadorID { get; set; }
            public string tipoEvento { get; set; } = string.Empty;
            public int minuto { get; set; }
            public string? descripcion { get; set; }
        }

        public class JugadorSimpleDto
        {
            public int jugadorID { get; set; }
            public int numeroCamiseta { get; set; }
            public string posicion { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
        }

        public class EquipoConJugadoresDto
        {
            public int equipoID { get; set; }
            public string nombreEquipo { get; set; } = string.Empty;
            public List<JugadorSimpleDto> jugadores { get; set; } = new();
        }

        public class JugadoresPartidoDto
        {
            public EquipoConJugadoresDto equipoLocal { get; set; } = new();
            public EquipoConJugadoresDto equipoVisitante { get; set; } = new();
        }

        private PartidoDto MapPartido(Partido p)
        {
            return new PartidoDto
            {
                partidoID = p.Part_Id,
                equipoLocalID = p.Equi_Id_Local,
                equipoVisitanteID = p.Equi_Id_Visitante,
                fechaPartido = p.Part_FechaPartido.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                lugarPartido = p.Sede_Nombre ?? string.Empty,
                golesLocal = p.Part_GolesLocal,
                golesVisitante = p.Part_GolesVisitante,
                estado = p.Part_Estado ?? string.Empty,
                arbitroID = p.Usua_Id,
                equipoLocal = new EquipoDto { equipoID = p.Equi_Id_Local, nombreEquipo = p.Equi_Nombre_Local ?? string.Empty },
                equipoVisitante = new EquipoDto { equipoID = p.Equi_Id_Visitante, nombreEquipo = p.Equi_Nombre_Visitante ?? string.Empty }
            };
        }

        private EventoPartidoDto MapEvento(Estadistica e)
        {
            string tipo = "";
            if ((e.EsPa_Goles ?? 0) > 0) tipo = "Gol";
            else if ((e.EsPa_Asistencias ?? 0) > 0) tipo = "Asistencia";
            else if ((e.EsPa_TarjetasAmarillas ?? 0) > 0) tipo = "TarjetaAmarilla";
            else if ((e.EsPa_TarjetasRojas ?? 0) > 0) tipo = "TarjetaRoja";

            return new EventoPartidoDto
            {
                eventoID = e.EsPa_Id,
                partidoID = e.Part_Id,
                jugadorID = e.Juga_Id,
                tipoEvento = tipo,
                minuto = e.EsPa_MinutosJugados ?? 0,
                descripcion = null // no se almacena descripción en BD
            };
        }

        // GET api/partido/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            try
            {
                var partido = await _partidosData.ObtenerPartido(id);
                if (partido == null)
                {
                    return NotFound(new { isSuccess = false, message = "Partido no encontrado" });
                }

                var dto = MapPartido(partido);
                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/partido/{id}/eventos
        [HttpGet("{id}/eventos")]
        public async Task<IActionResult> ObtenerEventos(int id)
        {
            try
            {
                var estadisticas = await _estadisticasData.ListarEstadisticasPorPartido(id);
                var lista = estadisticas.Select(MapEvento).ToList();
                return Ok(new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // POST api/partido/{id}/iniciar
        [HttpPost("{id}/iniciar")]
        [Authorize(Roles = "Árbitro")]
        public async Task<IActionResult> Iniciar(int id)
        {
            try
            {
                bool ok = await _partidosData.CambiarEstadoPartido(id, "En Curso");
                return Ok(new { success = ok, message = ok ? "Partido iniciado" : "No se pudo iniciar el partido" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST api/partido/{id}/finalizar
        [HttpPost("{id}/finalizar")]
        [Authorize(Roles = "Árbitro")]
        public async Task<IActionResult> Finalizar(int id)
        {
            try
            {
                bool ok = await _partidosData.FinalizarPartidoAutomatico(id);
                return Ok(new { success = ok, message = ok ? "Partido finalizado" : "No se pudo finalizar el partido" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST api/partido/evento
        [HttpPost("evento")]
        [Authorize(Roles = "Árbitro")]
        public async Task<IActionResult> RegistrarEvento([FromBody] RegistrarEventoRequest request)
        {
            try
            {
                var estadistica = new RegistrarEstadistica
                {
                    Part_Id = request.partidoID,
                    Juga_Id = request.jugadorID,
                    EsPa_Goles = 0,
                    EsPa_Asistencias = 0,
                    EsPa_TarjetasAmarillas = 0,
                    EsPa_TarjetasRojas = 0,
                    EsPa_MinutosJugados = request.minuto
                };

                switch (request.tipoEvento)
                {
                    case "Gol":
                        estadistica.EsPa_Goles = 1;
                        break;
                    case "Asistencia":
                        estadistica.EsPa_Asistencias = 1;
                        break;
                    case "TarjetaAmarilla":
                        estadistica.EsPa_TarjetasAmarillas = 1;
                        break;
                    case "TarjetaRoja":
                        estadistica.EsPa_TarjetasRojas = 1;
                        break;
                }

                int? id = await _estadisticasData.RegistrarEstadisticaYObtenerId(estadistica);

                if (id == null)
                {
                    return Ok(new { isSuccess = false, message = "No se pudo registrar el evento", data = (EventoPartidoDto?)null });
                }

                // Volver a leer la estadística para mapearla a evento
                var lista = await _estadisticasData.ListarEstadisticasPorPartido(request.partidoID);
                var stat = lista.FirstOrDefault(e => e.EsPa_Id == id.Value);
                var dto = stat != null ? MapEvento(stat) : null;

                return Ok(new { isSuccess = true, data = dto, message = "Evento registrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // DELETE api/partido/evento/{id}
        [HttpDelete("evento/{id}")]
        [Authorize(Roles = "Árbitro")]
        public async Task<IActionResult> EliminarEvento(int id)
        {
            try
            {
                bool ok = await _estadisticasData.EliminarEstadistica(id);
                return Ok(new { success = ok, message = ok ? "Evento eliminado" : "No se pudo eliminar el evento" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET api/partido/{id}/jugadores
        [HttpGet("{id}/jugadores")]
        public async Task<IActionResult> ObtenerJugadoresPartido(int id)
        {
            try
            {
                var partido = await _partidosData.ObtenerPartido(id);
                if (partido == null)
                {
                    return NotFound(new { isSuccess = false, message = "Partido no encontrado" });
                }

                var jugadoresLocal = await _jugadoresData.ListarJugadoresPorEquipo(partido.Equi_Id_Local);
                var jugadoresVisitante = await _jugadoresData.ListarJugadoresPorEquipo(partido.Equi_Id_Visitante);

                var equipoLocal = new EquipoConJugadoresDto
                {
                    equipoID = partido.Equi_Id_Local,
                    nombreEquipo = partido.Equi_Nombre_Local ?? string.Empty,
                    jugadores = jugadoresLocal.Select(j => new JugadorSimpleDto
                    {
                        jugadorID = j.Juga_Id,
                        numeroCamiseta = j.Juga_Numero ?? 0,
                        posicion = j.Juga_Posicion ?? string.Empty,
                        nombre = j.Usua_NombreCompleto ?? string.Empty
                    }).ToList()
                };

                var equipoVisitante = new EquipoConJugadoresDto
                {
                    equipoID = partido.Equi_Id_Visitante,
                    nombreEquipo = partido.Equi_Nombre_Visitante ?? string.Empty,
                    jugadores = jugadoresVisitante.Select(j => new JugadorSimpleDto
                    {
                        jugadorID = j.Juga_Id,
                        numeroCamiseta = j.Juga_Numero ?? 0,
                        posicion = j.Juga_Posicion ?? string.Empty,
                        nombre = j.Usua_NombreCompleto ?? string.Empty
                    }).ToList()
                };

                var dto = new JugadoresPartidoDto
                {
                    equipoLocal = equipoLocal,
                    equipoVisitante = equipoVisitante
                };

                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
