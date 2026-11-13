using Microsoft.AspNetCore.Mvc;
using Cine_TP.Domain;
using CineWebApi.DTOs;
using Cine_TP.Services.Interfaces;
namespace Cine_TP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobanteService _comprobanteService;

        // Inyectamos únicamente la capa de servicio
        public ComprobantesController(IComprobanteService comprobanteService)
        {
            _comprobanteService = comprobanteService;
        }

        // Viajes -------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var viajes = await _comprobanteService.ObtenerComprobantesAsync();
            return Ok(viajes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var viaje = await _comprobanteService.ObtenerComprobantePorIdAsync(id);
            if (viaje == null)
                return NotFound(new { mensaje = $"No se encontró el viaje con ID {id}." });

            return Ok(viaje);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Comprobante viaje)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (viaje.ComprobanteDetalles == null || !viaje.ComprobanteDetalles.Any())
                return BadRequest(new { mensaje = "Un viaje debe incluir al menos una excursión en sus detalles." });

            try
            {
                var resultado = await _comprobanteService.CrearComprobanteAsync(viaje);
                if (!resultado)
                    return StatusCode(500, new { mensaje = "Ocurrió un error al procesar el alta del viaje o faltan excursiones válidas." });

                return CreatedAtAction(nameof(GetById), new { id = viaje.Id }, viaje);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] ActualizarEstadoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actualizado = await _comprobanteService.ActualizarEstadoAsync(id, dto.NuevoEstado);
            if (!actualizado)
                return NotFound(new { mensaje = $"No se encontró el viaje con ID {id} para actualizar." });

            return Ok(new { mensaje = $"El estado del viaje {id} fue actualizado a '{dto.NuevoEstado}' correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _comprobanteService.BorrarComprobanteAsync(id);

            if (!exito)
                return BadRequest();

            return Ok();
        }
    }
}