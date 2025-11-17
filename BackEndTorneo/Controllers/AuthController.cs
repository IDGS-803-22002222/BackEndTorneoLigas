using BackEndTorneo.Data;
using BackEndTorneo.Helpers;
using BackEndTorneo.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BackEndTorneo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthData _authData;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AuthData authData, JwtHelper jwtHelper)
        {
            _authData = authData;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            try
            {
                var usuario = await _authData.Login(login.Email!);

                if (usuario == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Credenciales inválidas" });
                }

                var passwordHash = await _authData.ObtenerPasswordHash(login.Email!);

                if (!PasswordHelper.VerifyPassword(login.Password!, passwordHash!))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Credenciales inválidas" });
                }

                var token = _jwtHelper.GenerateToken(
                    usuario.Usua_Id,
                    usuario.Usua_Email!,
                    usuario.Rol_Id,
                    usuario.Rol_Nombre!
                );

                usuario.Token = token;

                await _authData.ActualizarUltimoAcceso(usuario.Usua_Id);

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register registro)
        {
            try
            {
                bool emailExiste = await _authData.VerificarEmailExiste(registro.Usua_Email!);

                if (emailExiste)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "El email ya está registrado" });
                }

                var passwordHash = PasswordHelper.HashPassword(registro.Usua_Password!);
                int usuaId = await _authData.Registrar(registro, passwordHash);

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, data = usuaId, message = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}