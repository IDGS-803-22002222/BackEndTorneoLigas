using BackEndTorneo.Data;
using BackEndTorneo.Models.Equipos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquiposController : ControllerBase
    {
        private readonly EquiposData _equiposData;

        public EquiposController(EquiposData equiposData)
        {
            _equiposData = equiposData;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Equipo> lista = await _equiposData.ListarEquipos();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{equipoId}")]
        public async Task<IActionResult> Obtener(int equipoId)
        {
            try
            {
                Equipo? equipo = await _equiposData.ObtenerEquipo(equipoId);

                if (equipo == null)
                {
                    return NotFound(new { isSuccess = false, message = "Equipo no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = equipo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Capitán")]
        public async Task<IActionResult> Crear([FromBody] CrearEquipo equipo)
        {
            try
            {
                int equipoId = await _equiposData.CrearEquipo(equipo);

                // Obtener el código QR generado
                string? codigoQR = await _equiposData.ObtenerCodigoQR(equipoId);

                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = equipoId,
                    codigoQR = codigoQR,
                    message = "Equipo creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador,Capitán")]
        public async Task<IActionResult> Editar([FromBody] Equipo equipo)
        {
            try
            {
                bool respuesta = await _equiposData.EditarEquipo(equipo);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{equipoId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int equipoId)
        {
            try
            {
                bool respuesta = await _equiposData.EliminarEquipo(equipoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("qr/{equipoId}")]
        public async Task<IActionResult> ObtenerQR(int equipoId)
        {
            try
            {
                string? codigoQR = await _equiposData.ObtenerCodigoQR(equipoId);

                if (codigoQR == null)
                {
                    return NotFound(new { isSuccess = false, message = "Equipo no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = codigoQR });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}