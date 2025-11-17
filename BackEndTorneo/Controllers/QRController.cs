using BackEndTorneo.Data;
using BackEndTorneo.Models.QR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRController : ControllerBase
    {
        private readonly QRData _qrData;

        public QRController(QRData qrData)
        {
            _qrData = qrData;
        }

        [HttpPost("generar-capitan")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GenerarQRCapitan()
        {
            try
            {
                string codigoQR = await _qrData.GenerarQRCapitan();
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = codigoQR,
                    message = "QR de capitán generado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("validar-capitan")]
        public async Task<IActionResult> ValidarQRCapitan([FromBody] ValidarQRCapitan validacion)
        {
            try
            {
                bool valido = await _qrData.ValidarYUsarQRCapitan(validacion);

                if (!valido)
                {
                    return BadRequest(new { isSuccess = false, message = "Código QR inválido o expirado" });
                }

                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    message = "Usuario convertido a capitán exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("capitanes")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListarQRCapitanes()
        {
            try
            {
                List<QRCapitan> lista = await _qrData.ListarQRCapitanes();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }


        [HttpPost("generar-equipo/{equipoId}")]
        [Authorize(Roles = "Administrador,Capitán")]
        public async Task<IActionResult> GenerarQREquipo(int equipoId)
        {
            try
            {
                string codigoQR = await _qrData.GenerarQREquipo(equipoId);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = codigoQR,
                    message = "QR de equipo generado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("equipos")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListarQREquipos()
        {
            try
            {
                List<QREquipo> lista = await _qrData.ListarQREquipos();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("equipo/{equipoId}")]
        public async Task<IActionResult> ObtenerQREquipo(int equipoId)
        {
            try
            {
                QREquipo? qr = await _qrData.ObtenerQREquipo(equipoId);

                if (qr == null)
                {
                    return NotFound(new { isSuccess = false, message = "QR no encontrado para este equipo" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = qr });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}