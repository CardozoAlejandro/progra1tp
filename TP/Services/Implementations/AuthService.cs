using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IUsuarioRepository _usuarioRepository;
        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration config)
        {
            _usuarioRepository = usuarioRepository;
            _config = config;
        }
        public async Task<bool> RegistrarAsync(string nombreUsuario, string email, string password, string dni)
        {
            if (await _usuarioRepository.GetUserByNameAsync(nombreUsuario) != null)
                throw new InvalidOperationException("El nombre de usuario ya existe.");
            if (await _usuarioRepository.GetUserByEmailAsync(email) != null)
                throw new InvalidOperationException("La dirección de email ya está en uso.");

                var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Rol = "Cliente"
            };

            if (!string.IsNullOrWhiteSpace(dni))
            {
                

                
            }

            await _usuarioRepository.CreateUserAsync(usuario);
            return true;
        }
        public async Task<string?> LoginAsync(string nombreUsuario, string password)
        {
            var usuario = await _usuarioRepository.GetUserByNameAsync(nombreUsuario);

            if (usuario == null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            { 
                return null; 
            }

            return GenerateJwtToken(usuario);
        }
        private string GenerateJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "CineWebApi",
                audience: "CineWebApi",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
