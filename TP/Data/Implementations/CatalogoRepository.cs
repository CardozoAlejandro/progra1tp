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
    public class CatalogoRepository : ICatalogoRepository
    {
        private readonly CineDbContext _context;
        public CatalogoRepository(CineDbContext context)
        {
            _context = context;
        }

        public async Task<List<Genero>> GetAllGenresAsync()
        {
            return await _context.Generos.OrderBy(g => g.NombreGenero).ToListAsync();
        }
                
        public async Task<List<Clasificacion>> GetAllClassificationsAsync()
        {
            return await _context.Clasificaciones.OrderBy(g => g.Codigo).ToListAsync();
        }
    }
}
