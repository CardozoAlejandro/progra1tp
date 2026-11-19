using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CineWebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/usuarios")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }
        private int? IdUsuarioActual =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _usuarioService.ObtenerUsuariosAsync();
            return Ok(usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo,
                FechaAlta = u.FechaAlta
            }));
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, bool estado)
        {
            if (id == IdUsuarioActual)
                return BadRequest(new { mensaje = "No podés cambiar el estado de tu propio usuario." });

            var actualizado = await _usuarioService.ActualizarEstadoUsuarioAsync(id, estado);
            if (!actualizado)
                return NotFound(new { mensaje = $"No se encontró el usuario con ID {id}." });

            return Ok(new { mensaje = "Estado actualizado." });
        }
    }
}
