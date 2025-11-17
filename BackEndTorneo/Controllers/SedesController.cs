using BackEndTorneo.Data;
using BackEndTorneo.Models.Sedes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SedesController : ControllerBase
    {
        private readonly SedesData _sedesData;

        public SedesController(SedesData sedesData)
        {
            _sedesData = sedesData;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Sede> lista = await _sedesData.ListarSedes();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{sedeId}")]
        public async Task<IActionResult> Obtener(int sedeId)
        {
            try
            {
                Sede? sede = await _sedesData.ObtenerSede(sedeId);

                if (sede == null)
                {
                    return NotFound(new { isSuccess = false, message = "Sede no encontrada" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = sede });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] Sede sede)
        {
            try
            {
                int sedeId = await _sedesData.CrearSede(sede);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = sedeId,
                    message = "Sede creada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar([FromBody] Sede sede)
        {
            try
            {
                bool respuesta = await _sedesData.EditarSede(sede);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{sedeId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int sedeId)
        {
            try
            {
                bool respuesta = await _sedesData.EliminarSede(sedeId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}