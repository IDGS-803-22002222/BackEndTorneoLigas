using BackEndTorneo.Data;
using BackEndTorneo.Models.Notificaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionesData _notificacionesData;

        public NotificacionesController(NotificacionesData notificacionesData)
        {
            _notificacionesData = notificacionesData;
        }

        [HttpGet("usuario/{usuaId}")]
        public async Task<IActionResult> ListarPorUsuario(int usuaId)
        {
            try
            {
                List<Notificacion> lista = await _notificacionesData.ListarNotificacionesPorUsuario(usuaId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("no-leidas/{usuaId}")]
        public async Task<IActionResult> ListarNoLeidas(int usuaId)
        {
            try
            {
                List<Notificacion> lista = await _notificacionesData.ListarNotificacionesNoLeidas(usuaId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("contar-no-leidas/{usuaId}")]
        public async Task<IActionResult> ContarNoLeidas(int usuaId)
        {
            try
            {
                int count = await _notificacionesData.ContarNotificacionesNoLeidas(usuaId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] CrearNotificacion notificacion)
        {
            try
            {
                int notiId = await _notificacionesData.CrearNotificacion(notificacion);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = notiId,
                    message = "Notificación creada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("masiva")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EnviarMasiva([FromBody] NotificacionMasiva notificacion)
        {
            try
            {
                bool respuesta = await _notificacionesData.EnviarNotificacionMasiva(notificacion);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = respuesta,
                    message = "Notificaciones enviadas exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut("marcar-leida/{notificacionId}")]
        public async Task<IActionResult> MarcarLeida(int notificacionId)
        {
            try
            {
                bool respuesta = await _notificacionesData.MarcarComoLeida(notificacionId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut("marcar-todas-leidas/{usuaId}")]
        public async Task<IActionResult> MarcarTodasLeidas(int usuaId)
        {
            try
            {
                bool respuesta = await _notificacionesData.MarcarTodasComoLeidas(usuaId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{notificacionId}")]
        public async Task<IActionResult> Eliminar(int notificacionId)
        {
            try
            {
                bool respuesta = await _notificacionesData.EliminarNotificacion(notificacionId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("notificar-proximo-partido/{partidoId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> NotificarProximoPartido(int partidoId)
        {
            try
            {
                await _notificacionesData.NotificarProximoPartido(partidoId);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    message = "Notificaciones enviadas a los jugadores"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}