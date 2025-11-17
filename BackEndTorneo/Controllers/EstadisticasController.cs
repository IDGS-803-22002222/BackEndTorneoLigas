using BackEndTorneo.Data;
using BackEndTorneo.Models.Estadisticas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticasController : ControllerBase
    {
        private readonly EstadisticasData _estadisticasData;

        public EstadisticasController(EstadisticasData estadisticasData)
        {
            _estadisticasData = estadisticasData;
        }

        [HttpGet("partido/{partidoId}")]
        public async Task<IActionResult> ObtenerPorPartido(int partidoId)
        {
            try
            {
                List<Estadistica> lista = await _estadisticasData.ListarEstadisticasPorPartido(partidoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("jugador/{jugadorId}")]
        public async Task<IActionResult> ObtenerPorJugador(int jugadorId)
        {
            try
            {
                List<Estadistica> lista = await _estadisticasData.ListarEstadisticasPorJugador(jugadorId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Árbitro")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarEstadistica estadistica)
        {
            try
            {
                bool respuesta = await _estadisticasData.RegistrarEstadistica(estadistica);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = respuesta,
                    message = "Estadística registrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("goleadores")]
        public async Task<IActionResult> ObtenerGoleadores([FromQuery] int? torneoId = null)
        {
            try
            {
                List<RankingGoleador> lista = await _estadisticasData.ObtenerRankingGoleadores(torneoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("tabla-posiciones/{torneoId}")]
        public async Task<IActionResult> ObtenerTablaPosiciones(int torneoId)
        {
            try
            {
                List<TablaPosicion> lista = await _estadisticasData.ObtenerTablaPosiciones(torneoId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}