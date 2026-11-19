using Cine_TP.Domain;
using Cine_TP.Services.Implementations;
using Cine_TP.Services.Interfaces;
using CineWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CineWebApi.Controllers
{
    [Authorize]
    [Route("api/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaService _peliculaService;
        public PeliculasController(IPeliculaService peliculaService)
        {
            _peliculaService = peliculaService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var peliculas = await _peliculaService.ObtenerPeliculasAsync();
            return Ok(peliculas);
        }

        [AllowAnonymous]
        [HttpGet("en-cartelera")]
        public async Task<IActionResult> GetAllActive([FromQuery(Name = "titulo")] string? nombrePelicula)
        {
            var peliculas = await _peliculaService.ObtenerPeliculasActivasAsync(nombrePelicula);
            var resultado = peliculas.Select(p => new PeliculaCarteleraDTO
            {
                Id = p.Id,
                Titulo = p.Titulo,
                DuracionMinutos = p.DuracionMinutos,
                FechaEstreno = p.FechaEstreno,
                Genero = p.Genero?.NombreGenero,
                Clasificacion = p.Clasificacion?.Codigo
            });

            return Ok(resultado);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pelicula = await _peliculaService.ObtenerPeliculaPorIdAsync(id);
            if (pelicula == null)
            {
                return NotFound(new { mensaje = $"No se encontró la película con ID {id}." });
            }
            return Ok(pelicula);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Pelicula pelicula)
        {
            try
            {
                var resultado = await _peliculaService.CrearPeliculaAsync(pelicula);
                if (!resultado)
                    return BadRequest();

                return Ok(new { mensaje = "Película creada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id,[FromQuery] string estado)
        {
            var actualizado = await _peliculaService.ActualizarEstadoPeliculaAsync(id, estado);
            if (!actualizado)
            {
                return NotFound();
            }

            return Ok(new { mensaje = $"El estado de la pelicula {id} fue actualizado a '{estado}' correctamente." });
        }
    }
}
