using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CineWebApi.Controllers
{
    [Route("api/clientes")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        private int? IdClienteActual =>
            int.TryParse(User.FindFirstValue("idCliente"), out var id) ? id : null;

        private int? IdUsuarioActual =>
            int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("idUsuario") ??
                User.FindFirstValue("sub"),
                out var id) ? id : null;

        private static ClienteDTO ADto(Cliente c) => new()
        {
            Id = c.Id,
            Dni = c.Dni,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Email = c.Email,
            Telefono = c.Telefono,
            FechaNacimiento = c.FechaNacimiento,
            IdTipoPublico = c.IdTipoPublico
        };

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMio()
        {
            if (IdClienteActual is null)
                return Forbid();

            var cliente = await _clienteService.ObtenerClientePorIdAsync(IdClienteActual.Value);
            if (cliente == null)
                return NotFound(new { mensaje = "No se encontró tu perfil de cliente." });

            return Ok(ADto(cliente));
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> ActualizarMio([FromBody] ActualizarPerfilClienteDTO dto)
        {
            if (IdClienteActual is null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Apellido) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { mensaje = "Nombre, apellido y email son obligatorios." });
            }

            try
            {
                var actualizado = await _clienteService.ActualizarPerfilAsync(
                    IdClienteActual.Value,
                    dto.Nombre.Trim(),
                    dto.Apellido.Trim(),
                    dto.Email.Trim(),
                    string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim()
                );

                if (actualizado == null)
                    return NotFound(new { mensaje = "No se encontró tu perfil de cliente." });

                return Ok(ADto(actualizado));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("me/password")]
        public async Task<IActionResult> CambiarPasswordMio([FromBody] CambiarPasswordDTO dto)
        {
            if (IdClienteActual is null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.PasswordActual) || string.IsNullOrWhiteSpace(dto.PasswordNueva))
                return BadRequest(new { mensaje = "Debés ingresar la contraseña actual y la nueva contraseña." });

            if (dto.PasswordNueva.Length < 6)
                return BadRequest(new { mensaje = "La nueva contraseña debe tener al menos 6 caracteres." });

            try
            {
                var ok = await _clienteService.CambiarPasswordAsync(
                    IdClienteActual.Value,
                    IdUsuarioActual,
                    dto.PasswordActual,
                    dto.PasswordNueva
                );

                if (!ok)
                    return BadRequest(new { mensaje = "La contraseña actual es incorrecta o no se pudo actualizar." });

                return Ok(new { mensaje = "Contraseña actualizada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _clienteService.ObtenerClientesAsync();
            return Ok(clientes.Select(ADto));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.ObtenerClientePorIdAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = $"No se encontró el cliente con ID {id}." });

            return Ok(ADto(cliente));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dni")]
        public async Task<IActionResult> GetByDni([FromQuery] string dni)
        {
            var cliente = await _clienteService.ObtenerClientePorDniAsync(dni);
            if (cliente == null)
                return NotFound(new { mensaje = "No se encontró un cliente con ese DNI." });

            return Ok(ADto(cliente));
        }
    }



}
