using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface IPeliculaService
    {
        Task<List<Pelicula>> ObtenerPeliculasAsync();
        Task<Pelicula?> ObtenerPeliculaPorIdAsync(int id);
        Task<List<Pelicula?>> ObtenerPeliculaPorNombreAsync(string nombrePelicula);
        Task<bool> CrearPeliculaAsync(Pelicula pelicula);
        Task<bool> ActualizarEstadoPeliculaAsync(int id, string estado);
        Task<bool> ActualizarPeliculaAsync(int id, Pelicula pelicula);
    }
}
