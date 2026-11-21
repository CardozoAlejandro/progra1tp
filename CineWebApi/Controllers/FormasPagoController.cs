using Cine_TP.Data;
using Cine_TP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineWebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/formas-pago")]
    [ApiController]
    public class FormasPagoController : ControllerBase
    {
        private readonly IFormaPagoService _service;
        public FormasPagoController(IFormaPagoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _service.ObtenerFormasPagoAsync();
            return Ok(lista.Select(f => new { id = f.Id, nombre = f.Nombre }));
        }
    }
}
