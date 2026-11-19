using Cine_TP.Data;
using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }
        public Task<bool> EmailExisteAsync(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult(false);
            }
            return _usuarioRepository.EmailExistsAsync(email);
        }

        public Task<Usuario?> ObtenerUsuarioPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }
            return _usuarioRepository.GetUserByEmailAsync(email);
        }

        public Task<Usuario?> ObtenerUsuarioPorId(int id)
        {
            if(id <= 0)
            {
                return null;
            }
            return _usuarioRepository.GetUserByIdAsync(id);
        }

        public Task<Usuario?> ObtenerUsuarioPorNombre(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return null;
            }
            return _usuarioRepository.GetUserByNameAsync(usuario);
        }

        public Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            return _usuarioRepository.GetAllUsersAsync();
        }

        public Task<bool> UsuarioExisteAsync(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Task.FromResult(false);
            }
            return _usuarioRepository.UserExistsAsync(usuario);
        }

        public Task<bool> ActualizarEstadoUsuarioAsync(int id, bool estado)
        {
            if (id <= 0)
            {
                return Task.FromResult(false);
            }
            return _usuarioRepository.UpdateUserStateAsync(id, estado);
        }
    }
}
