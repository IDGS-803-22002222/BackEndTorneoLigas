using BackEndTorneo.Data;
using BackEndTorneo.Models.Partidos;
using BackEndTorneo.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BackEndTorneo.Controllers
{
    [Route("api/arbitro")]
    [ApiController]
    [Authorize(Roles = "Árbitro")]
    public class ArbitroAppController : ControllerBase
    {
        private readonly UsuariosData _usuariosData;
        private readonly PartidosData _partidosData;
        private readonly EstadisticasData _estadisticasData;

        public ArbitroAppController(
            UsuariosData usuariosData,
            PartidosData partidosData,
            EstadisticasData estadisticasData)
        {
            _usuariosData = usuariosData;
            _partidosData = partidosData;
            _estadisticasData = estadisticasData;
        }

        private int ObtenerUsuarioIdDelToken()
        {
            // ASP.NET Core JWT middleware mapea 'sub' a ClaimTypes.NameIdentifier
            var subClaim = User.Claims.FirstOrDefault(c => 
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || 
                c.Type == JwtRegisteredClaimNames.Sub || 
                c.Type == "sub");
            if (subClaim == null)
                throw new Exception("No se pudo obtener el usuario desde el token");
            return int.Parse(subClaim.Value);
        }

        public class UsuarioDto
        {
            public int usua_Id { get; set; }
            public string? usua_NombreCompleto { get; set; }
            public string? usua_Email { get; set; }
            public string? usua_Telefono { get; set; }
            public int rol_Id { get; set; }
            public string? rol_Nombre { get; set; }
        }

        public class ArbitroDto
        {
            public int arbitroID { get; set; }
            public int usuarioID { get; set; }
            public string licencia { get; set; } = string.Empty;
            public string fechaRegistro { get; set; } = string.Empty;
            public bool activo { get; set; }
            public UsuarioDto? usuario { get; set; }
        }

        public class EstadisticasArbitroDto
        {
            public int arbitroID { get; set; }
            public int partidosArbitrados { get; set; }
            public int tarjetasAmarillasOtorgadas { get; set; }
            public int tarjetasRojasOtorgadas { get; set; }
            public double promedioTarjetasPorPartido { get; set; }
        }

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

        private UsuarioDto MapUsuario(Usuario u)
        {
            return new UsuarioDto
            {
                usua_Id = u.Usua_Id,
                usua_NombreCompleto = u.Usua_NombreCompleto,
                usua_Email = u.Usua_Email,
                usua_Telefono = u.Usua_Telefono,
                rol_Id = u.Rol_Id,
                rol_Nombre = u.Rol_Nombre
            };
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

        // GET api/arbitro/mi-perfil
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> MiPerfil()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                var usuario = await _usuariosData.ObtenerUsuario(usuaId) ?? throw new Exception("Usuario no encontrado");

                var dto = new ArbitroDto
                {
                    arbitroID = usuaId,
                    usuarioID = usuaId,
                    licencia = "General", // No se almacena en BD, valor simbólico
                    fechaRegistro = (usuario.Usua_FechaRegistro ?? DateTime.Now).ToString("yyyy-MM-dd"),
                    activo = usuario.Usua_Activo ?? true,
                    usuario = MapUsuario(usuario)
                };

                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/arbitro/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();

                var partidos = await _partidosData.ListarPartidos();
                var arbitrados = partidos.Where(p => p.Usua_Id == usuaId && p.Part_Estado == "Finalizado").ToList();

                int partidosArbitrados = arbitrados.Count;
                int amarillas = 0;
                int rojas = 0;

                foreach (var p in arbitrados)
                {
                    var stats = await _estadisticasData.ListarEstadisticasPorPartido(p.Part_Id);
                    amarillas += stats.Sum(e => e.EsPa_TarjetasAmarillas ?? 0);
                    rojas += stats.Sum(e => e.EsPa_TarjetasRojas ?? 0);
                }

                double promedio = partidosArbitrados > 0 ? (double)(amarillas + rojas) / partidosArbitrados : 0;

                var dto = new EstadisticasArbitroDto
                {
                    arbitroID = usuaId,
                    partidosArbitrados = partidosArbitrados,
                    tarjetasAmarillasOtorgadas = amarillas,
                    tarjetasRojasOtorgadas = rojas,
                    promedioTarjetasPorPartido = promedio
                };

                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/arbitro/proximos-partidos
        [HttpGet("proximos-partidos")]
        public async Task<IActionResult> ProximosPartidos()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                var proximos = await _partidosData.ObtenerProximosPartidos();
                var asignados = proximos.Where(p => p.Usua_Id == usuaId).Select(MapPartido).ToList();
                return Ok(new { isSuccess = true, data = asignados });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/arbitro/partidos
        [HttpGet("partidos")]
        public async Task<IActionResult> Partidos()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                var partidos = await _partidosData.ListarPartidos();
                var asignados = partidos.Where(p => p.Usua_Id == usuaId).Select(MapPartido).ToList();
                return Ok(new { isSuccess = true, data = asignados });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        public class ActualizarArbitroRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Telefono { get; set; }
        }

        // PUT api/arbitro/actualizar-perfil
        [HttpPut("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarArbitroRequest request)
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                bool ok = await _usuariosData.ActualizarPerfilBasico(usuaId, request.Nombre, request.Email, request.Telefono);

                return Ok(new
                {
                    success = ok,
                    message = ok ? "Perfil actualizado exitosamente" : "No se pudo actualizar el perfil"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
