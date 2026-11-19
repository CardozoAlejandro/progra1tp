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
        private const int CapacidadSala = 50;
        private readonly IFuncionService _funcionService;
        public FuncionesController(IFuncionService funcionService)
        {
            _funcionService = funcionService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var f = await _funcionService.ObtenerFuncionPorIdAsync(id);
            if (f == null)
                return NotFound(new { mensaje = $"No se encontró la función con ID {id}." });

            var vendidas = await _funcionService.ContarEntradasVendidasAsync(id);

            return Ok(new FuncionDetalleDTO
            {
                IdFuncion = f.Id,
                IdPelicula = f.IdPelicula,
                Titulo = f.Pelicula!.Titulo,
                DuracionMinutos = f.Pelicula.DuracionMinutos,
                Genero = f.Pelicula.Genero?.NombreGenero,
                Clasificacion = f.Pelicula.Clasificacion?.Codigo,
                IdSala = f.IdSala,
                FechaHoraInicio = f.FechaHoraInicio,
                PrecioBase = f.PrecioBase,
                Capacidad = CapacidadSala,
                Disponibles = Math.Max(0, CapacidadSala - vendidas)
            });
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