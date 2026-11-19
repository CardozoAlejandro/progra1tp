using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Implementations
{
    public class PeliculaRepository : IPeliculaRepository
    {
        private readonly CineDbContext _context;
        public PeliculaRepository(CineDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateMovieAsync(Pelicula pelicula)
        {
            await _context.Peliculas.AddAsync(pelicula);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Pelicula>> GetAllMoviesAsync()
        {
            return await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Clasificacion)
                .ToListAsync();
        }

        public async Task<Pelicula?> GetMovieByIdAsync(int id)
        {
            return await _context.Peliculas
                .Where(p => p.Id == id)
                .Include(p => p.Genero)
                .Include(p => p.Clasificacion)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Pelicula>> GetMovieByNameAsync(string nombrePelicula)
        {
            if (string.IsNullOrWhiteSpace(nombrePelicula))
            {
                return new List<Pelicula>();
            }
            return await _context.Peliculas
                .Where(p => p.Titulo.Contains(nombrePelicula))
                .Include(p => p.Genero)
                .Include(p => p.Clasificacion)
                .ToListAsync();
        }   

        public async Task<bool> UpdateMovieAsync(int id, Pelicula pelicula)
        {
            var peli = await _context.Peliculas
                                         .Where(p => p.Id == id)
                                         .FirstOrDefaultAsync();
            if (peli == null || pelicula == null)
            {
                return false;
            }
            peli.Titulo = pelicula.Titulo;
            peli.DuracionMinutos = pelicula.DuracionMinutos;
            peli.Sinopsis = pelicula.Sinopsis;
            peli.FechaEstreno = pelicula.FechaEstreno;
            peli.IdGenero = pelicula.IdGenero;
            peli.IdClasificacion = pelicula.IdClasificacion;
            peli.Estado = pelicula.Estado;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMovieStateAsync(int id, string estado)
        {
            var pelicula = await _context.Peliculas
                                         .Where(p => p.Id == id)
                                         .FirstOrDefaultAsync();
            if (pelicula == null)
            {
                return false;
            }
            pelicula.Estado = estado;
            return await _context.SaveChangesAsync() > 0; 
        }
    }
}
