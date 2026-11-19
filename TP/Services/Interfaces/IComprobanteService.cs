using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Domain;

namespace Cine_TP.Services.Interfaces
{
    public interface IComprobanteService
    {
        Task<List<Comprobante>> ObtenerComprobantesAsync();
        Task<Comprobante?> ObtenerComprobantePorIdAsync(int id);
        Task<bool> CrearComprobanteAsync(Comprobante comprobante);
        Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado);
        Task<bool> BorrarComprobanteAsync(int id);
        Task<List<(string pelicula, decimal total)>> ObtenerIngresosPorPeliculaAsync();
        Task<List<(int anio, int mes, decimal total)>> ObtenerIngresosMensualesAsync();
    }
}
