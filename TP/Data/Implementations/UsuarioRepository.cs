using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;

namespace Cine_TP.Data.Implementations
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly CineDbContext _context;
        public UsuarioRepository(CineDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllUsersAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            return await _context.Usuarios
                                 .Where(u => u.Email == email)
                                 .FirstOrDefaultAsync();
        }

        public async Task<Usuario?> GetUserByIdAsync(int id)
        {
            return await _context.Usuarios
                                 .Where(u => u.Id == id)
                                 .FirstOrDefaultAsync();
        }

        public async Task<Usuario?> GetUserByNameAsync(string usuario)
        {
            return await _context.Usuarios
                                 .Include(u => u.Cliente)
                                 .Where(u => u.NombreUsuario == usuario)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> UserExistsAsync(string usuario)
        {
            return await _context.Usuarios
                                 .AnyAsync(u => u.NombreUsuario == usuario);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Usuarios
                                 .AnyAsync(u => u.Email == email);
        }
        public async Task<bool> UpdateUserStateAsync(int id, bool estado)
        {
            var usuario = await _context.Usuarios
                                         .Where(u => u.Id == id)
                                         .FirstOrDefaultAsync();
            if (usuario == null)
            {
                return false;
            }
            usuario.Activo = estado;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateUserAsync(Usuario usuario)
        {
            if(usuario == null)
            {
                return false;
            }
            try
            {
                await _context.Usuarios.AddAsync(usuario);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
