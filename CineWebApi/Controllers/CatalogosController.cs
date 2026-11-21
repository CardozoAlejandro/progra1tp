using Cine_TP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineWebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/catalogo")]
    [ApiController]
    public class CatalogosController : ControllerBase
    {
        private readonly ICatalogoService _catalogoService;
        public CatalogosController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        [HttpGet("generos")]
        public async Task<IActionResult> Generos()
        {
            var generos = await _catalogoService.ObtenerGenerosAsync();
            return Ok(generos.Select(g => new { id = g.Id, nombreGenero = g.NombreGenero }));
        }

        [HttpGet("clasificaciones")]
        public async Task<IActionResult> Clasificaciones()
        {
            var clasificaciones = await _catalogoService.ObtenerClasificacionesAsync();
            return Ok(clasificaciones.Select(c => new { id = c.Id, codigo = c.Codigo }));
        }
    }
}