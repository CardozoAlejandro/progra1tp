using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Cine_TP.Services.Interfaces.IComprobanteService;

namespace Cine_TP.Services.Implementations
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly IFuncionRepository _funcionRepository;
        public ComprobanteService(IComprobanteRepository comprobanteRepository, IFuncionRepository funcionRepository)
        {
            _comprobanteRepository = comprobanteRepository;
            _funcionRepository = funcionRepository;
        }
        public Task<List<Comprobante>> ObtenerComprobantesAsync()
        {
            return _comprobanteRepository.GetAllComprobantesAsync();
        }

        public Task<Comprobante?> ObtenerComprobantePorIdAsync(int id)
        {
            if(id <= 0)
            {
                return Task.FromResult<Comprobante?>(null);
            }
            return _comprobanteRepository.GetComprobanteByIdAsync(id);
        }

        public Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado)
        {
            if (id <= 0)
            {
                return Task.FromResult(false);
            }
            if(!EstadoComprobante.Validos.Contains(nuevoEstado))
            {
                return Task.FromResult(false);
            }
            return _comprobanteRepository.UpdateEstadoAsync(id, nuevoEstado);
        }

        public Task<bool> CrearComprobanteAsync(Comprobante comp)
        {
            if (comp.IdCliente is null or <= 0)
            {
                return Task.FromResult(false);
            }
            if (comp.IdFormaPago is null or <= 0)
            { 
            return Task.FromResult(false); 
            }
            if (!EstadoComprobante.Validos.Contains(comp.Estado))
            {
                return Task.FromResult(false);
            }
            if (comp.ComprobanteDetalles == null || comp.ComprobanteDetalles.Count == 0)
            {
                return Task.FromResult(false);
            }
            if (comp.Estado == EstadoComprobante.Confirmada && comp.IdFormaPago is null or <= 0)
            {
                return Task.FromResult(false);
            }
            comp.FechaHoraCreacion = DateTime.Now;
            comp.FechaLimiteConfirmacion ??= DateTime.Now.AddMinutes(15);
            return _comprobanteRepository.CreateComprobanteAsync(comp);
        }

        public Task<bool> BorrarComprobanteAsync(int id)
        {
            if(id <= 0)
            {
                return Task.FromResult(false);
            }
            return _comprobanteRepository.DeleteComprobanteAsync(id);
        }
        public Task<List<(string pelicula, decimal total)>> ObtenerIngresosPorPeliculaAsync()
        {
            return _comprobanteRepository.GetIngresosAsync();
        }

        public async Task<List<(int anio, int mes, decimal total)>> ObtenerIngresosMensualesAsync()
        {
            var porMes = await _comprobanteRepository.GetIngresosMensualesAsync();
            if (porMes.Count == 0)
                return new List<(int, int, decimal)>();

            var desde = new DateTime(porMes[0].anio, porMes[0].mes, 1);
            var hasta = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var resultado = new List<(int Anio, int Mes, decimal Total)>();
            for (var m = desde; m <= hasta; m = m.AddMonths(1))
            {
                var fila = porMes.FirstOrDefault(x => x.anio == m.Year && x.mes == m.Month);
                resultado.Add((m.Year, m.Month, fila.total));
            }
            return resultado;
        }

        public async Task<ResultadoCompra> ReservarEntradasAsync(int idFuncion, int cantidad, int? idUsuario, string? nombreUsuario)
        {
            if (cantidad <= 0)
                return new(null, 400, "La cantidad debe ser mayor a cero.");

            var idCliente = await _comprobanteRepository.GetIdClienteAsync(idUsuario, nombreUsuario);
            if (idCliente == null)
                return new(null, 400, "El usuario no tiene un cliente asociado.");

            var funcion = await _funcionRepository.GetFuncionByIdAsync(idFuncion);
            if (funcion == null)
                return new(null, 404, "No se encontró la función.");
            if (funcion.FechaHoraInicio <= DateTime.Now)
                return new(null, 409, "Esta función ya comenzó.");

            var butacas = await _funcionRepository.GetButacasLibresAsync(funcion.Id, funcion.IdSala, cantidad);
            if (butacas.Count < cantidad)
                return new(null, 409, "No hay suficientes entradas disponibles.");

            var comprobante = new Comprobante
            {
                IdCliente = idCliente,
                Estado = EstadoComprobante.Pendiente,
                FechaLimiteConfirmacion = DateTime.Now.AddMinutes(10),
                ComprobanteDetalles = butacas.Select(b => new ComprobanteDetalle
                {
                    IdFuncion = funcion.Id,
                    IdButaca = b.Id,
                    PrecioBase = funcion.PrecioBase
                }).ToList()
            };

            try
            {
                var ok = await CrearComprobanteAsync(comprobante);
                return ok
                    ? new(comprobante)
                    : new(null, 400, "No se pudo registrar la compra.");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is DbUpdateException)
            {
                return new(null, 409, "Las entradas fueron tomadas por otra persona. Intentá de nuevo.");
            }
        }
    }
}
