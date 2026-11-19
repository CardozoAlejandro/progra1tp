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
        Task<List<(string pelicula, decimal total)>> GetIngresosAsync();
        Task<List<(int anio, int mes, decimal total)>> GetIngresosMensualesAsync();
        Task<int?> GetIdClienteAsync(int? idUsuario, string? nombreUsuario);
        Task<bool> FormaPagoExistsAsync(int id);
    }
}
