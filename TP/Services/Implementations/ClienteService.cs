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
    }
}
