using Cine_TP.Data.Implementations;
using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CineWebApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDTO registro)
        {
            try
            {
                await _authService.RegistrarAsync(registro.NombreUsuario, registro.Email, registro.Password,
                    registro.Nombre, registro.Apellido, registro.Dni, registro.FechaNacimiento, registro.Telefono);

                return StatusCode(201, new { mensaje = "Usuario creado." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _authService.LoginAsync(login.NombreUsuario, login.Password);
            if (token == null)
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            return Ok(new { Token = token });
        }
    }
}
