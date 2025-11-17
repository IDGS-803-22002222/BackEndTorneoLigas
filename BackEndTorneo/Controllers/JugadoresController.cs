using BackEndTorneo.Data;
using BackEndTorneo.Models.Jugadores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadoresController : ControllerBase
    {
        private readonly JugadoresData _jugadoresData;

        public JugadoresController(JugadoresData jugadoresData)
        {
            _jugadoresData = jugadoresData;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Jugador> lista = await _jugadoresData.ListarJugadores();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("equipo/{equipoId}")]
        public async Task<IActionResult> ListarPorEquipo(int equipoId)
        {
            try
            {
                List<Jugador> lista = await _jugadoresData.ListarJugadoresPorEquipo(equipoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("inscribir")]
        public async Task<IActionResult> Inscribir([FromBody] InscribirJugador inscripcion)
        {
            try
            {
                // Validar QR y obtener equipo
                int? equipoId = await _jugadoresData.ValidarYObtenerEquipoDeQR(inscripcion.CodigoQR!);

                if (equipoId == null)
                {
                    return BadRequest(new { isSuccess = false, message = "Código QR inválido o equipo inactivo" });
                }

                bool respuesta = await _jugadoresData.InscribirJugador(inscripcion, equipoId.Value);

                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = respuesta,
                    message = respuesta ? "Jugador inscrito exitosamente" : "Error al inscribir jugador"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{jugadorId}")]
        [Authorize(Roles = "Administrador,Capitán")]
        public async Task<IActionResult> Eliminar(int jugadorId)
        {
            try
            {
                bool respuesta = await _jugadoresData.EliminarJugador(jugadorId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}