// BackEndTorneo/Controllers/ArbitrosController.cs
using BackEndTorneo.Data;
using BackEndTorneo.Helpers;
using BackEndTorneo.Models.Auth;
using BackEndTorneo.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class ArbitrosController : ControllerBase
    {
        private readonly AuthData _authData;
        private readonly UsuariosData _usuariosData;

        public ArbitrosController(AuthData authData, UsuariosData usuariosData)
        {
            _authData = authData;
            _usuariosData = usuariosData;
        }

        [HttpGet]
        public async Task<IActionResult> ListarArbitros()
        {
            try
            {
                List<Usuario> lista = await _usuariosData.ObtenerUsuariosPorRolTodos(4);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerArbitro(int id)
        {
            try
            {
                Usuario? arbitro = await _usuariosData.ObtenerUsuario(id);

                if (arbitro == null || arbitro.Rol_Id != 4)
                {
                    return NotFound(new { isSuccess = false, message = "Árbitro no encontrado" });
                }

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = arbitro });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearArbitro([FromBody] Register registro)
        {
            try
            {
                // Verificar que el rol sea de árbitro
                registro.Rol_Id = 4;

                bool emailExiste = await _authData.VerificarEmailExiste(registro.Usua_Email!);

                if (emailExiste)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new { isSuccess = false, message = "El email ya está registrado" });
                }

                var passwordHash = PasswordHelper.HashPassword(registro.Usua_Password!);
                int usuaId = await _authData.Registrar(registro, passwordHash);

                return StatusCode(StatusCodes.Status200OK,
                    new { isSuccess = true, data = usuaId, message = "Árbitro registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditarArbitro([FromBody] EditarUsuario usuario)
        {
            try
            {
                // Verificar que sea un árbitro
                var arbitroExistente = await _usuariosData.ObtenerUsuario(usuario.Usua_Id);
                if (arbitroExistente == null || arbitroExistente.Rol_Id != 4)
                {
                    return BadRequest(new { isSuccess = false, message = "No es un árbitro válido" });
                }

                bool respuesta = await _usuariosData.EditarUsuario(usuario);
                return StatusCode(StatusCodes.Status200OK,
                    new { isSuccess = respuesta, message = "Árbitro actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarArbitro(int id)
        {
            try
            {
                // Verificar que sea un árbitro
                var arbitro = await _usuariosData.ObtenerUsuario(id);
                if (arbitro == null || arbitro.Rol_Id != 4)
                {
                    return BadRequest(new { isSuccess = false, message = "No es un árbitro válido" });
                }

                bool respuesta = await _usuariosData.EliminarUsuario(id);
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}