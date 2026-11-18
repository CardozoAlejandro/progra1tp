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
        private readonly IClienteRepository _clienteRepository;
        public AuthService(IUsuarioRepository usuarioRepository, IClienteRepository clienteRepository, IConfiguration config)
        {
            _usuarioRepository = usuarioRepository;
            _clienteRepository = clienteRepository;
            _config = config;
        }
        public async Task<bool> RegistrarAsync(string nombreUsuario, string email, string password, string nombre, string apellido, string dni, DateTime fechaNacimiento, string? telefono)
        {
            if (await _usuarioRepository.GetUserByNameAsync(nombreUsuario) != null)
                throw new InvalidOperationException("El nombre de usuario ya existe.");
            if (await _usuarioRepository.GetUserByEmailAsync(email) != null)
                throw new InvalidOperationException("La dirección de email ya está en uso.");
            if (fechaNacimiento.Date > DateTime.Today)
                throw new InvalidOperationException("La fecha de nacimiento no es válida.");

            dni = dni.Replace(".", "").Replace(" ", "").Trim();

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Rol = "Cliente"
            };

            var cliente = await _clienteRepository.GetByDniAsync(dni);

            if (cliente == null)
            {
                usuario.Cliente = new Cliente
                {
                    Dni = dni,
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    Telefono = telefono,
                    FechaNacimiento = fechaNacimiento
                };
            }
            else
            {
                if (cliente.IdUsuario != null || cliente.FechaNacimiento.Date != fechaNacimiento.Date)
                    throw new InvalidOperationException("No se pudo vincular el DNI con los datos indicados.");

                usuario.Cliente = cliente;
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

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (usuario.Cliente is not null)
                claims.Add(new Claim("idCliente", usuario.Cliente.Id.ToString()));

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
