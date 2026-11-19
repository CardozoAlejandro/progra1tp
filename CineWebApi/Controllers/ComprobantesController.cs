using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Cine_TP.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobanteService _comprobanteService;

        public ComprobantesController(IComprobanteService comprobanteService)
        {
            _comprobanteService = comprobanteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comprobantes = await _comprobanteService.ObtenerComprobantesAsync();
            return Ok(comprobantes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comprobante = await _comprobanteService.ObtenerComprobantePorIdAsync(id);
            if (comprobante == null)
                return NotFound(new { mensaje = $"No se encontró el comprobante con ID {id}." });

            return Ok(comprobante);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearComprobanteDTO dto)
        {
            int? idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id : (int?)null;

            var r = await _comprobanteService.ReservarEntradasAsync(dto.IdFuncion, dto.Cantidad, idUsuario, User.Identity?.Name);
            if (r.Comprobante == null)
                return StatusCode(r.Status, new { mensaje = r.Mensaje });

            var c = r.Comprobante;
            return Ok(new ComprobanteCreadoDTO
            {
                IdComprobante = c.Id,
                Estado = c.Estado,
                Cantidad = dto.Cantidad,
                MontoTotal = c.MontoTotal,
                FechaLimiteConfirmacion = c.FechaLimiteConfirmacion
            });
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromQuery] string estado)
        {
            var actualizado = await _comprobanteService.ActualizarEstadoAsync(id, estado);
            if (!actualizado)
                return NotFound(new { mensaje = $"No se encontró el comprobante con ID {id} para actualizar." });

            return Ok(new { mensaje = $"El estado del comprobante {id} fue actualizado a '{estado}' correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _comprobanteService.BorrarComprobanteAsync(id);

            if (!exito)
                return NotFound();

            return Ok();
        }

        [HttpGet("ingresos-por-pelicula")]
        public async Task<IActionResult> IngresosPorPelicula()
        {
            var datos = await _comprobanteService.ObtenerIngresosPorPeliculaAsync();
            return Ok(datos.Select(x => new IngresoPeliculaDTO
            {
                Pelicula = x.pelicula,
                Total = x.total
            }));
        }

        [HttpGet("ingresos-mensuales")]
        public async Task<IActionResult> IngresosMensuales()
        {
            var datos = await _comprobanteService.ObtenerIngresosMensualesAsync();
            return Ok(datos.Select(x => new IngresoMensualDTO
            {
                Anio = x.anio,
                Mes = x.mes,
                Total = x.total
            }));
        }
    }
}