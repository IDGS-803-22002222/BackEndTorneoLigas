using BackEndTorneo.Data;
using BackEndTorneo.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosData _usuariosData;

        public UsuariosController(UsuariosData usuariosData)
        {
            _usuariosData = usuariosData;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Listar()
        {
            try
            {
                List<Usuario> lista = await _usuariosData.ListarUsuarios();
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{usuaId}")]
        public async Task<IActionResult> Obtener(int usuaId)
        {
            try
            {
                Usuario? usuario = await _usuariosData.ObtenerUsuario(usuaId);

                if (usuario == null)
                {
                    return NotFound(new { isSuccess = false, message = "Usuario no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] EditarUsuario usuario)
        {
            try
            {
                bool respuesta = await _usuariosData.EditarUsuario(usuario);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    isSuccess = respuesta,
                    message = "Perfil actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{usuaId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int usuaId)
        {
            try
            {
                bool respuesta = await _usuariosData.EliminarUsuario(usuaId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("rol/{rolId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerPorRol(int rolId)
        {
            try
            {
                List<Usuario> lista = await _usuariosData.ObtenerUsuariosPorRol(rolId);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}