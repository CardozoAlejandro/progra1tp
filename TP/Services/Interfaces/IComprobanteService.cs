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
        public record ResultadoCompra(Comprobante? Comprobante, int Status = 200, string? Mensaje = null);
        Task<List<Comprobante>> ObtenerComprobantesAsync();
        Task<Comprobante?> ObtenerComprobantePorIdAsync(int id);
        Task<bool> CrearComprobanteAsync(Comprobante comprobante);
        Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado);
        Task<bool> BorrarComprobanteAsync(int id);
        Task<List<(string pelicula, decimal total)>> ObtenerIngresosPorPeliculaAsync();
        Task<List<(int anio, int mes, decimal total)>> ObtenerIngresosMensualesAsync();
        Task<bool> ExisteFormaPagoAsync(int id);
        Task<ResultadoCompra> ReservarEntradasAsync(int idFuncion, int cantidad, int idFormaPago, int? idUsuario, string? nombreUsuario);
        Task<List<Comprobante>> ObtenerMisComprobantesAsync(int? idUsuario, string? nombreUsuario);
    }
}
