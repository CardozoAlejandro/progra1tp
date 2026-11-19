using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(int id);
        Task<Cliente?> GetByDniAsync(string dni);
        Task UpdateAsync(Cliente cliente);
    }
}
