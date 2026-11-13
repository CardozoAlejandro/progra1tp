using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Domain;

namespace Cine_TP.Data.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAllUsersAsync();
        Task<Usuario?> GetUserByIdAsync(int id);
        Task<Usuario?> GetUserByNameAsync(string usuario);
        Task<Usuario?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string usuario);
        Task<bool> EmailExistsAsync(string email);
        Task<Usuario?> ValidateUserAsync(string nombreUsuario, string passwordHash);
    }
}