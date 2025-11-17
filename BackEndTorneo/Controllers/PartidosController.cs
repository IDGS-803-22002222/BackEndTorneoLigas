using BackEndTorneo.Data;
using BackEndTorneo.Models.Partidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidosController : ControllerBase
    {
        private readonly PartidosData _partidosData;

        public PartidosController(PartidosData partidosData)
        {
            _partidosData = partidosData;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Partido> lista = await _partidosData.ListarPartidos();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("torneo/{torneoId}")]
        public async Task<IActionResult> ListarPorTorneo(int torneoId)
        {
            try
            {
                List<Partido> lista = await _partidosData.ListarPartidosPorTorneo(torneoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{partidoId}")]
        public async Task<IActionResult> Obtener(int partidoId)
        {
            try
            {
                Partido? partido = await _partidosData.ObtenerPartido(partidoId);

                if (partido == null)
                {
                    return NotFound(new { isSuccess = false, message = "Partido no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = partido });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("proximos")]
        public async Task<IActionResult> ObtenerProximos()
        {
            try
            {
                List<Partido> lista = await _partidosData.ObtenerProximosPartidos();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] CrearPartido partido)
        {
            try
            {
                int partidoId = await _partidosData.CrearPartido(partido);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = true,
                    data = partidoId,
                    message = "Partido creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut("resultado")]
        [Authorize(Roles = "Administrador,Árbitro")]
        public async Task<IActionResult> RegistrarResultado([FromBody] RegistrarResultado resultado)
        {
            try
            {
                bool respuesta = await _partidosData.RegistrarResultado(resultado);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = respuesta,
                    message = "Resultado registrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar([FromBody] Partido partido)
        {
            try
            {
                bool respuesta = await _partidosData.EditarPartido(partido);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}