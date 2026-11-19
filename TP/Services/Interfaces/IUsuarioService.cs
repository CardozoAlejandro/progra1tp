using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerUsuariosAsync();
        Task<Usuario?> ObtenerUsuarioPorId(int id);
        Task<Usuario?> ObtenerUsuarioPorNombre(string usuario);
        Task<Usuario?> ObtenerUsuarioPorEmail(string email);
        Task<bool> UsuarioExisteAsync(string usuario);
        Task<bool> EmailExisteAsync(string email);
        Task<Usuario?> ValidarUsuarioAsync(string nombreUsuario, string passwordHash);
    }
}
