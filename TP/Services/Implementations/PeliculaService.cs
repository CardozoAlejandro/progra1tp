using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class PeliculaService : IPeliculaService
    {
        private readonly IPeliculaRepository _peliculaRepository;
        public PeliculaService(IPeliculaRepository peliculaRepository)
        {
            _peliculaRepository = peliculaRepository;
        }

        public Task<bool> ActualizarEstadoPeliculaAsync(int id, string estado)
        {
            if(id <= 0 || !EstadoPelicula.Validos.Contains(estado))
            {
                return Task.FromResult(false);
            }
            return _peliculaRepository.UpdateMovieStateAsync(id, estado);
        }

        public Task<bool> ActualizarPeliculaAsync(int id, Pelicula pelicula)
        {
            if(!ValidarPelicula(pelicula))
            {
                return Task.FromResult(false);
            }
            return _peliculaRepository.UpdateMovieAsync(id, pelicula);
        }

        public Task<bool> CrearPeliculaAsync(Pelicula pelicula)
        {
            if(!ValidarPelicula(pelicula))
            {
                return Task.FromResult(false);
            }
            return _peliculaRepository.CreateMovieAsync(pelicula);
        }

        public async Task<Pelicula?> ObtenerPeliculaPorIdAsync(int id)
        {
            if(id <= 0)
            {
                return null;
            }
            return await _peliculaRepository.GetMovieByIdAsync(id);
        }

        public Task<List<Pelicula>> ObtenerPeliculaPorNombreAsync(string nombrePelicula)
        {
            return _peliculaRepository.GetMovieByNameAsync(nombrePelicula);
        }

        public Task<List<Pelicula>> ObtenerPeliculasAsync()
        {
            return _peliculaRepository.GetAllMoviesAsync();
        }

        public bool ValidarPelicula(Pelicula pelicula)
        {
            if (pelicula == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(pelicula.Titulo))
            {
                return false;
            }
            if (pelicula.DuracionMinutos < 0)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(pelicula.Sinopsis))
            {
                return false;
            }
            if (pelicula.FechaEstreno is null || pelicula.FechaEstreno > DateTime.Now)
            {
                return false;
            }
            if (!EstadoPelicula.Validos.Contains(pelicula.Estado))
            {
                return false;
            }
            if (pelicula.IdGenero is null || pelicula.IdGenero <= 0)
            {
                return false;
            }
            if (pelicula.IdClasificacion is null || pelicula.IdClasificacion <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
