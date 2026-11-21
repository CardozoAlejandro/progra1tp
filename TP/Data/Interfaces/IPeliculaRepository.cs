using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Domain;

namespace Cine_TP.Data.Interfaces
{
    public interface IPeliculaRepository
    {
        Task<List<Pelicula>> GetAllMoviesAsync();
        Task<List<Pelicula>> GetAllActiveMoviesAsync(string? nombrePelicula);
        Task<Pelicula?> GetMovieByIdAsync(int id);
        Task<bool> CreateMovieAsync(Pelicula pelicula);
        Task<bool> UpdateMovieStateAsync(int id, string estado);
        Task<bool> UpdateMovieAsync(int id, Pelicula pelicula);
    }
}
