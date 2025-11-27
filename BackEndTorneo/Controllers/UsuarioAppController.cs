using BackEndTorneo.Data;
using BackEndTorneo.Helpers;
using BackEndTorneo.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BackEndTorneo.Controllers
{
    [Route("api/usuario")]
    [ApiController]
    [Authorize]
    public class UsuarioAppController : ControllerBase
    {
        private readonly UsuariosData _usuariosData;
        private readonly AuthData _authData;

        public UsuarioAppController(UsuariosData usuariosData, AuthData authData)
        {
            _usuariosData = usuariosData;
            _authData = authData;
        }

        private int ObtenerUsuarioIdDelToken()
        {
            // ASP.NET Core JWT middleware mapea 'sub' a ClaimTypes.NameIdentifier
            var subClaim = User.Claims.FirstOrDefault(c => 
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || 
                c.Type == JwtRegisteredClaimNames.Sub || 
                c.Type == "sub");
            if (subClaim == null)
            {
                throw new Exception("No se pudo obtener el usuario desde el token");
            }

            return int.Parse(subClaim.Value);
        }

        // GET api/usuario/perfil
        [HttpGet("perfil")]
        public async Task<IActionResult> ObtenerPerfil()
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                Usuario? usuario = await _usuariosData.ObtenerUsuario(usuaId);

                if (usuario == null)
                {
                    return NotFound(new { isSuccess = false, message = "Usuario no encontrado" });
                }

                return Ok(new { isSuccess = true, data = usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        public class UpdateUsuarioRequest
        {
            public string Nombre { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string? Telefono { get; set; }
        }

        // PUT api/usuario/actualizar
        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] UpdateUsuarioRequest request)
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();

                bool actualizado = await _usuariosData.ActualizarPerfilBasico(usuaId, request.Nombre, request.Email, request.Telefono);

                return Ok(new
                {
                    success = actualizado,
                    message = actualizado ? "Perfil actualizado exitosamente" : "No se pudo actualizar el perfil"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class CambiarPasswordRequest
        {
            public string PasswordActual { get; set; } = null!;
            public string PasswordNuevo { get; set; } = null!;
        }

        // PUT api/usuario/cambiar-password
        [HttpPut("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            try
            {
                int usuaId = ObtenerUsuarioIdDelToken();
                var usuario = await _usuariosData.ObtenerUsuario(usuaId);

                if (usuario == null || string.IsNullOrEmpty(usuario.Usua_Email))
                {
                    return Unauthorized(new { success = false, message = "Usuario no válido" });
                }

                var passwordHashActual = await _authData.ObtenerPasswordHash(usuario.Usua_Email);
                if (passwordHashActual == null || !PasswordHelper.VerifyPassword(request.PasswordActual, passwordHashActual))
                {
                    return Ok(new { success = false, message = "La contraseña actual es incorrecta" });
                }

                var nuevoHash = PasswordHelper.HashPassword(request.PasswordNuevo);
                bool actualizado = await _authData.ActualizarPassword(usuaId, nuevoHash);

                return Ok(new
                {
                    success = actualizado,
                    message = actualizado ? "Contraseña actualizada exitosamente" : "No se pudo actualizar la contraseña"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
