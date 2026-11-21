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
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public Task<Cliente?> ObtenerClientePorDniAsync(string dni)
        {
            if(string.IsNullOrWhiteSpace(dni))
            {
                return Task.FromResult<Cliente?>(null);
            }
            dni = dni.Replace(".", "").Replace(" ", "").Trim();
            return _clienteRepository.GetByDniAsync(dni);
        }

        public Task<Cliente?> ObtenerClientePorIdAsync(int id)
        {
            if(id <= 0)
            {
                return Task.FromResult<Cliente?>(null);
            }
            return _clienteRepository.GetByIdAsync(id);
        }

        public Task<List<Cliente>> ObtenerClientesAsync()
        {
            return _clienteRepository.GetAllAsync();
        }
        public async Task<Cliente?> ActualizarPerfilAsync(int idCliente, string nombre, string apellido, string email, string? telefono)
        {
            if (idCliente <= 0)
            {
                return null;
            }

            var cliente = await _clienteRepository.GetByIdAsync(idCliente);
            if (cliente == null)
            {
                return null;
            }

            cliente.Nombre = nombre;
            cliente.Apellido = apellido;
            cliente.Email = email;
            cliente.Telefono = telefono;

            if (cliente.Usuario != null)
            {
                cliente.Usuario.Email = email;
            }

            await _clienteRepository.UpdateAsync(cliente);

            return cliente;
        }

        public async Task<bool> CambiarPasswordAsync(int idCliente, int? idUsuario, string passwordActual, string passwordNueva)
        {
            if (idCliente <= 0 || string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(passwordNueva))
            {
                return false;
            }

            var cliente = await _clienteRepository.GetByIdAsync(idCliente);
            if (cliente?.Usuario == null)
            {
                return false;
            }

            if (idUsuario.HasValue && cliente.Usuario.Id != idUsuario.Value)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(cliente.Usuario.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(passwordActual, cliente.Usuario.PasswordHash))
            {
                return false;
            }

            if (passwordActual == passwordNueva ||
                BCrypt.Net.BCrypt.Verify(passwordNueva, cliente.Usuario.PasswordHash))
            {
                throw new InvalidOperationException("La nueva contraseña no puede ser igual a la contraseña actual.");
            }

            cliente.Usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);

            await _clienteRepository.UpdateAsync(cliente);

            return true;
        }
    }
}
