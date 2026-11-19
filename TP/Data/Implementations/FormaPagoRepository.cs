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
    public class FormaPagoRepository : IFormaPagoRepository
    {
        private readonly CineDbContext _context;
        public FormaPagoRepository(CineDbContext context)
        {
            _context = context;
        }
        public async Task<List<FormaPago>> GetFormasPagoAsync()
        {
            return await _context.FormasPago
                .AsNoTracking()
                .OrderBy(f => f.Nombre)
                .ToListAsync();
        }
    }
}
