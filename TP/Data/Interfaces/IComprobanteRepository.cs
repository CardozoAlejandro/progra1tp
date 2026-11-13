using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Domain;

namespace Cine_TP.Data.Interfaces
{
    public interface IComprobanteRepository
    {
        Task<List<Comprobante>> GetAllComprobantesAsync();
        Task<Comprobante?> GetComprobanteByIdAsync(int id);
        Task<bool> CreateComprobanteAsync(Comprobante comprobante);
        Task<bool> UpdateEstadoAsync(int id, string nuevoEstado);
        Task<bool> DeleteComprobanteAsync(int id);
    }
}
