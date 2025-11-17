using BackEndTorneo.Data;
using BackEndTorneo.Models.Torneos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorneosController : ControllerBase
    {
        private readonly TorneosData _torneosData;

        public TorneosController(TorneosData torneosData)
        {
            _torneosData = torneosData;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Torneo> lista = await _torneosData.ListarTorneos();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{torneoId}")]
        public async Task<IActionResult> Obtener(int torneoId)
        {
            try
            {
                Torneo? torneo = await _torneosData.ObtenerTorneo(torneoId);

                if (torneo == null)
                {
                    return NotFound(new { isSuccess = false, message = "Torneo no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = torneo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] CrearTorneo torneo)
        {
            try
            {
                int torneoId = await _torneosData.CrearTorneo(torneo);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = torneoId,
                    message = "Torneo creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar([FromBody] Torneo torneo)
        {
            try
            {
                bool respuesta = await _torneosData.EditarTorneo(torneo);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{torneoId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int torneoId)
        {
            try
            {
                bool respuesta = await _torneosData.EliminarTorneo(torneoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("inscribir-equipo")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> InscribirEquipo([FromBody] InscribirEquipoTorneo inscripcion)
        {
            try
            {
                bool respuesta = await _torneosData.InscribirEquipo(inscripcion);

                if (!respuesta)
                {
                    return BadRequest(new { isSuccess = false, message = "El equipo ya está inscrito en este torneo" });
                }

                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    message = "Equipo inscrito exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{torneoId}/equipos")]
        public async Task<IActionResult> ObtenerEquipos(int torneoId)
        {
            try
            {
                var equipos = await _torneosData.ObtenerEquiposDelTorneo(torneoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = equipos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut("{torneoId}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int torneoId, [FromBody] string estado)
        {
            try
            {
                bool respuesta = await _torneosData.CambiarEstadoTorneo(torneoId, estado);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}