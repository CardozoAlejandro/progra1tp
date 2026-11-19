using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineWebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/funciones")]
    [ApiController]
    public class FuncionesController : ControllerBase
    {
        private readonly IFuncionService _funcionService;
        public FuncionesController(IFuncionService funcionService)
        {
            _funcionService = funcionService;
        }

        [HttpGet("horarios")]
        public async Task<IActionResult> GetHorarios([FromQuery] int dias = 3)
        {
            dias = Math.Clamp(dias, 1, 14);
            var desde = DateTime.Now;
            var hasta = DateTime.Today.AddDays(dias);

            var funciones = await _funcionService.ObtenerFuncionesAsync(desde, hasta);
            return Ok(funciones.Select(f => new FuncionHorarioDTO
            {
                IdFuncion = f.Id,
                IdPelicula = f.IdPelicula,
                FechaHoraInicio = f.FechaHoraInicio
            }));
        }

        [HttpGet("pelicula/{idPelicula}")]
        public async Task<IActionResult> GetPorPelicula(int idPelicula)
        {
            var funciones = await _funcionService.ObtenerFuncionesPorPeliculaAsync(idPelicula, DateTime.Now);
            return Ok(funciones.Select(f => new FuncionHorarioDTO
            {
                IdFuncion = f.Id,
                IdPelicula = f.IdPelicula,
                FechaHoraInicio = f.FechaHoraInicio
            }));
        }
    }
}