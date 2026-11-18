using Microsoft.AspNetCore.Mvc;
using Cine_TP.Domain;
using CineWebApi.DTOs;
using Cine_TP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Create([FromBody] Comprobante comprobante)
        {
            if (comprobante.ComprobanteDetalles == null || !comprobante.ComprobanteDetalles.Any())
                return BadRequest(new { mensaje = "Un comprobante debe incluir al menos un detalle." });

            try
            {
                var resultado = await _comprobanteService.CrearComprobanteAsync(comprobante);
                if (!resultado)
                    return StatusCode(500, new { mensaje = "Ocurrió un error al procesar el alta del comprobante." });

                return CreatedAtAction(nameof(GetById), new { id = comprobante.Id }, comprobante);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
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
    }
}