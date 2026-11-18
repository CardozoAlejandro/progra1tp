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

        [Authorize(Roles = "Cliente")]
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
