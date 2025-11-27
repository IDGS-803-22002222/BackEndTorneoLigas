using BackEndTorneo.Data;
using BackEndTorneo.Models.Jugadores;
using BackEndTorneo.Models.Equipos;
using BackEndTorneo.Models.Estadisticas;
using BackEndTorneo.Models.Partidos;
using BackEndTorneo.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BackEndTorneo.Controllers
{
    [Route("api/jugador")]
    [ApiController]
    [Authorize(Roles = "Jugador,Capitán")]
    public class JugadorAppController : ControllerBase
    {
        private readonly JugadoresData _jugadoresData;
        private readonly EquiposData _equiposData;
        private readonly UsuariosData _usuariosData;
        private readonly EstadisticasData _estadisticasData;
        private readonly PartidosData _partidosData;
        private readonly QRData _qrData;

        public JugadorAppController(
            JugadoresData jugadoresData,
            EquiposData equiposData,
            UsuariosData usuariosData,
            EstadisticasData estadisticasData,
            PartidosData partidosData,
            QRData qrData)
        {
            _jugadoresData = jugadoresData;
            _equiposData = equiposData;
            _usuariosData = usuariosData;
            _estadisticasData = estadisticasData;
            _partidosData = partidosData;
            _qrData = qrData;
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

        private string ObtenerRolNombreDelToken()
        {
            var rolClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            return rolClaim?.Value ?? string.Empty;
        }

        // DTOs que coinciden con los modelos de Android
        public class UsuarioDto
        {
            public int usua_Id { get; set; }
            public string? usua_NombreCompleto { get; set; }
            public string? usua_Email { get; set; }
            public string? usua_Telefono { get; set; }
            public int rol_Id { get; set; }
            public string? rol_Nombre { get; set; }
        }

        public class EquipoDto
        {
            public int equipoID { get; set; }
            public string nombreEquipo { get; set; } = string.Empty;
            public string? logo { get; set; }
            public string fechaCreacion { get; set; } = string.Empty;
            public int capitanID { get; set; }
            public bool activo { get; set; }
        }

        public class JugadorDto
        {
            public int jugadorID { get; set; }
            public int usuarioID { get; set; }
            public int? equipoID { get; set; }
            public int numeroCamiseta { get; set; }
            public string posicion { get; set; } = string.Empty;
            public bool esCapitan { get; set; }
            public string fechaIngreso { get; set; } = string.Empty;
            public bool activo { get; set; }
            public UsuarioDto? usuario { get; set; }
            public EquipoDto? equipo { get; set; }
        }

        public class EstadisticasJugadorDto
        {
            public int jugadorID { get; set; }
            public int partidosJugados { get; set; }
            public int goles { get; set; }
            public int asistencias { get; set; }
            public int tarjetasAmarillas { get; set; }
            public int tarjetasRojas { get; set; }
            public double promedioGoles { get; set; }
            public JugadorDto? jugador { get; set; }
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

        private EquipoDto MapEquipo(Equipo e)
        {
            return new EquipoDto
            {
                equipoID = e.Equi_Id,
                nombreEquipo = e.Equi_Nombre ?? string.Empty,
                logo = e.Equi_Logo,
                fechaCreacion = (e.Equi_FechaCreacion ?? DateTime.Now).ToString("yyyy-MM-dd"),
                capitanID = e.Usua_Id ?? 0,
                activo = e.Equi_Activo ?? true
            };
        }

        private JugadorDto MapJugador(Jugador j, Usuario u, Equipo? e, bool esCapitan)
        {
            return new JugadorDto
            {
                jugadorID = j.Juga_Id,
                usuarioID = j.Usua_Id,
                equipoID = j.Equi_Id,
                numeroCamiseta = j.Juga_Numero ?? 0,
                posicion = j.Juga_Posicion ?? string.Empty,
                esCapitan = esCapitan,
                fechaIngreso = (j.Juga_FechaInscripcion ?? DateTime.Now).ToString("yyyy-MM-dd"),
                activo = j.Juga_Activo ?? true,
                usuario = MapUsuario(u),
                equipo = e != null ? MapEquipo(e) : null
            };
        }

        // GET api/jugador/mi-perfil
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> MiPerfil()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                var usuario = await _usuariosData.ObtenerUsuario(usuaId) ?? throw new Exception("Usuario no encontrado");
                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);

                Equipo? equipo = null;
                bool esCapitan = rolNombre == "Capitán";

                if (jugador != null)
                {
                    equipo = await _equiposData.ObtenerEquipo(jugador.Equi_Id);
                    if (equipo != null && equipo.Usua_Id == usuaId)
                    {
                        esCapitan = true;
                    }
                }
                else if (rolNombre == "Capitán")
                {
                    // Capitán sin registro en Jugadores: construir perfil virtual
                    equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                    if (equipo != null)
                    {
                        jugador = new Jugador
                        {
                            Juga_Id = 0,
                            Usua_Id = usuaId,
                            Usua_NombreCompleto = usuario.Usua_NombreCompleto,
                            Equi_Id = equipo.Equi_Id,
                            Equi_Nombre = equipo.Equi_Nombre,
                            Juga_Numero = 0,
                            Juga_Posicion = "Capitán",
                            Juga_FechaInscripcion = equipo.Equi_FechaCreacion,
                            Juga_Activo = true
                        };
                    }
                }

                if (jugador == null)
                {
                    // Jugador aún sin equipo / inscripción
                    jugador = new Jugador
                    {
                        Juga_Id = 0,
                        Usua_Id = usuaId,
                        Usua_NombreCompleto = usuario.Usua_NombreCompleto,
                        Equi_Id = equipo?.Equi_Id ?? 0,
                        Equi_Nombre = equipo?.Equi_Nombre,
                        Juga_Numero = 0,
                        Juga_Posicion = esCapitan ? "Capitán" : "Sin posición",
                        Juga_FechaInscripcion = usuario.Usua_FechaRegistro,
                        Juga_Activo = true
                    };
                }

                var dto = MapJugador(jugador, usuario, equipo, esCapitan);

                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/jugador/equipo  (y alias mi-equipo)
        [HttpGet("equipo")]
        [HttpGet("mi-equipo")]
        public async Task<IActionResult> MiEquipo()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                Equipo? equipo = null;

                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                if (jugador != null)
                {
                    equipo = await _equiposData.ObtenerEquipo(jugador.Equi_Id);
                }
                else if (rolNombre == "Capitán")
                {
                    equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                }

                if (equipo == null)
                {
                    return Ok(new { isSuccess = true, data = (EquipoDto?)null });
                }

                var dto = MapEquipo(equipo);
                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/jugador/equipo/jugadores
        [HttpGet("equipo/jugadores")]
        public async Task<IActionResult> JugadoresDeMiEquipo()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                Equipo? equipo = null;
                var jugadorActual = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                if (jugadorActual != null)
                {
                    equipo = await _equiposData.ObtenerEquipo(jugadorActual.Equi_Id);
                }
                else if (rolNombre == "Capitán")
                {
                    equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                }

                if (equipo == null)
                {
                    return Ok(new { isSuccess = true, data = new List<JugadorDto>() });
                }

                var jugadores = await _jugadoresData.ListarJugadoresPorEquipo(equipo.Equi_Id);

                // Mapear con información mínima de usuario usando sólo nombre
                var lista = new List<JugadorDto>();
                foreach (var j in jugadores)
                {
                    var usuario = await _usuariosData.ObtenerUsuario(j.Usua_Id) ?? new Usuario
                    {
                        Usua_Id = j.Usua_Id,
                        Usua_NombreCompleto = j.Usua_NombreCompleto
                    };
                    bool esCapitan = equipo.Usua_Id == j.Usua_Id;
                    lista.Add(MapJugador(j, usuario, equipo, esCapitan));
                }

                return Ok(new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/jugador/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                
                // Si es capitán sin registro de jugador, devolver estadísticas vacías
                if (jugador == null)
                {
                    if (rolNombre == "Capitán")
                    {
                        return Ok(new { isSuccess = true, data = new EstadisticasJugadorDto
                        {
                            jugadorID = 0,
                            partidosJugados = 0,
                            goles = 0,
                            asistencias = 0,
                            tarjetasAmarillas = 0,
                            tarjetasRojas = 0,
                            promedioGoles = 0,
                            jugador = null
                        }});
                    }
                    return Ok(new { isSuccess = true, data = (EstadisticasJugadorDto?)null });
                }

                var estadisticas = await _estadisticasData.ListarEstadisticasPorJugador(jugador.Juga_Id);

                int partidosJugados = estadisticas.Select(e => e.Part_Id).Distinct().Count();
                int goles = estadisticas.Sum(e => e.EsPa_Goles ?? 0);
                int asistencias = estadisticas.Sum(e => e.EsPa_Asistencias ?? 0);
                int amarillas = estadisticas.Sum(e => e.EsPa_TarjetasAmarillas ?? 0);
                int rojas = estadisticas.Sum(e => e.EsPa_TarjetasRojas ?? 0);

                double promedioGoles = partidosJugados > 0 ? (double)goles / partidosJugados : 0;

                var usuario = await _usuariosData.ObtenerUsuario(usuaId) ?? new Usuario { Usua_Id = usuaId };
                var equipo = await _equiposData.ObtenerEquipo(jugador.Equi_Id);
                bool esCapitan = equipo?.Usua_Id == usuaId;
                var jugadorDto = MapJugador(jugador, usuario, equipo, esCapitan);

                var dto = new EstadisticasJugadorDto
                {
                    jugadorID = jugador.Juga_Id,
                    partidosJugados = partidosJugados,
                    goles = goles,
                    asistencias = asistencias,
                    tarjetasAmarillas = amarillas,
                    tarjetasRojas = rojas,
                    promedioGoles = promedioGoles,
                    jugador = jugadorDto
                };

                return Ok(new { isSuccess = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        private PartidoDto MapPartido(Partido p)
        {
            var dto = new PartidoDto
            {
                partidoID = p.Part_Id,
                equipoLocalID = p.Equi_Id_Local,
                equipoVisitanteID = p.Equi_Id_Visitante,
                fechaPartido = p.Part_FechaPartido.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                lugarPartido = p.Sede_Nombre ?? string.Empty,
                golesLocal = p.Part_GolesLocal,
                golesVisitante = p.Part_GolesVisitante,
                estado = p.Part_Estado ?? string.Empty,
                arbitroID = p.Usua_Id
            };

            // Opcionalmente se podrían mapear equipoLocal / equipoVisitante si se requieren
            return dto;
        }

        // GET api/jugador/partidos
        [HttpGet("partidos")]
        public async Task<IActionResult> Partidos()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                int? equipoId = null;
                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                if (jugador != null)
                {
                    equipoId = jugador.Equi_Id;
                }
                else if (rolNombre == "Capitán")
                {
                    var equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                    equipoId = equipo?.Equi_Id;
                }

                if (equipoId == null)
                {
                    return Ok(new { isSuccess = true, data = new List<PartidoDto>() });
                }

                var todos = await _partidosData.ListarPartidos();
                var filtrados = todos.Where(p => p.Equi_Id_Local == equipoId || p.Equi_Id_Visitante == equipoId).ToList();
                var lista = filtrados.Select(MapPartido).ToList();

                return Ok(new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        // GET api/jugador/proximos-partidos
        [HttpGet("proximos-partidos")]
        public async Task<IActionResult> ProximosPartidos()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                int? equipoId = null;
                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                if (jugador != null)
                {
                    equipoId = jugador.Equi_Id;
                }
                else if (rolNombre == "Capitán")
                {
                    var equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                    equipoId = equipo?.Equi_Id;
                }

                if (equipoId == null)
                {
                    return Ok(new { isSuccess = true, data = new List<PartidoDto>() });
                }

                var proximos = await _partidosData.ObtenerProximosPartidos();
                var filtrados = proximos.Where(p => p.Equi_Id_Local == equipoId || p.Equi_Id_Visitante == equipoId).ToList();
                var lista = filtrados.Select(MapPartido).ToList();

                return Ok(new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        public class UpdateJugadorRequest
        {
            public int NumeroCamiseta { get; set; }
            public string Posicion { get; set; } = string.Empty;
        }

        // PUT api/jugador/actualizar
        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarJugador([FromBody] UpdateJugadorRequest request)
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                bool ok = await _jugadoresData.ActualizarJugadorPorUsuario(usuaId, request.NumeroCamiseta, request.Posicion);

                return Ok(new
                {
                    success = ok,
                    message = ok ? "Perfil de jugador actualizado exitosamente" : "No se pudo actualizar el jugador"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class UpdatePerfilCompletoRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Telefono { get; set; }
            public int NumeroCamiseta { get; set; }
            public string Posicion { get; set; } = string.Empty;
        }

        // PUT api/jugador/actualizar-perfil-completo
        [HttpPut("actualizar-perfil-completo")]
        public async Task<IActionResult> ActualizarPerfilCompleto([FromBody] UpdatePerfilCompletoRequest request)
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                // Actualizar datos básicos de usuario
                bool okUsuario = await _usuariosData.ActualizarPerfilBasico(usuaId, request.Nombre, request.Email, request.Telefono);

                // Actualizar jugador (solo si tiene registro en Jugadores, no para capitán sin jugador)
                bool okJugador = true; // Por defecto true para capitanes sin registro
                var jugador = await _jugadoresData.ObtenerJugadorPorUsuario(usuaId);
                if (jugador != null)
                {
                    okJugador = await _jugadoresData.ActualizarJugadorPorUsuario(usuaId, request.NumeroCamiseta, request.Posicion);
                }

                bool ok = okUsuario && okJugador;

                return Ok(new
                {
                    success = ok,
                    message = ok ? "Perfil actualizado exitosamente" : "No se pudo actualizar completamente el perfil"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST api/jugador/equipo/generar-qr
        [HttpPost("equipo/generar-qr")]
        public async Task<IActionResult> GenerarQrEquipo()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                string rolNombre = ObtenerRolNombreDelToken();

                if (rolNombre != "Capitán")
                {
                    return BadRequest(new { isSuccess = false, message = "Solo el capitán puede generar códigos QR" });
                }

                var equipo = await _equiposData.ObtenerEquipoPorCapitan(usuaId);
                if (equipo == null)
                {
                    return BadRequest(new { isSuccess = false, message = "No se encontró un equipo asociado al capitán" });
                }

                string codigoQR = await _qrData.GenerarQREquipo(equipo.Equi_Id);

                return Ok(new { isSuccess = true, data = codigoQR, message = "QR generado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
