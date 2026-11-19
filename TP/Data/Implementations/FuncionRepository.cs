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
    public class FuncionRepository : IFuncionRepository
    {
        private readonly CineDbContext _context;
        public FuncionRepository(CineDbContext context)
        {
            _context = context;
        }
        public async Task<List<Funcion>> GetFuncionesAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Funciones
                .AsNoTracking()
                .Where(f => f.FechaHoraInicio >= desde && f.FechaHoraInicio < hasta)
                .OrderBy(f => f.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<List<Funcion>> GetFuncionesPeliculasAsync(int idPelicula, DateTime desde)
        {
            return await _context.Funciones
                .AsNoTracking()
                .Where(f => f.IdPelicula == idPelicula && f.FechaHoraInicio >= desde)
                .OrderBy(f => f.FechaHoraInicio)
                .ToListAsync();
        }
    }
}
