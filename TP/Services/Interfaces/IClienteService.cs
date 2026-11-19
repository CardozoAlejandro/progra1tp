using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface IClienteService
    {
        Task<List<Cliente>> ObtenerClientesAsync();
        Task<Cliente?> ObtenerClientePorIdAsync(int id);
        Task<Cliente?> ObtenerClientePorDniAsync(string dni);
    }
}
