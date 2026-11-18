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
    public class ClienteRepository : IClienteRepository
    {
        private readonly CineDbContext _context;
        public ClienteRepository(CineDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<Cliente?> GetByDniAsync(string dni)
        {
            return await _context.Clientes.Include(c => c.Usuario)
                                          .Where(c => c.Dni == dni)
                                          .FirstOrDefaultAsync();
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes.Include(c => c.Usuario)
                                          .Where(c => c.Id == id)
                                          .FirstOrDefaultAsync();
        }
    }
}
