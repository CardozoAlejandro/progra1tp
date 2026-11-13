using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        public ComprobanteService(IComprobanteRepository comprobanteRepository)
        {
            _comprobanteRepository = comprobanteRepository;
        }
        public Task<List<Comprobante>> ObtenerComprobantesAsync()
        {
            return _comprobanteRepository.GetAllComprobantesAsync();
        }

        public Task<Comprobante?> ObtenerComprobantePorIdAsync(int id)
        {
            if(id < 0)
            {
                return null;
            }
            return _comprobanteRepository.GetComprobanteByIdAsync(id);
        }

        public Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado)
        {
            if (id < 0)
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
            if(comp.IdCliente is null || comp.IdCliente < 0)
            {
                return Task.FromResult(false);
            }
            if(comp.FechaHoraCreacion > DateTime.Now)
            {
                return Task.FromResult(false);
            }
            if(comp.FechaLimiteConfirmacion <  DateTime.Now)
            {
                return Task.FromResult(false);
            }
            if(comp.FechaHoraEmision > DateTime.Now)
            {
                return Task.FromResult(false);
            }
            if(!EstadoComprobante.Validos.Contains(comp.Estado))
            {
                return Task.FromResult(false);
            }
            if(comp.IdFormaPago is null || comp.IdFormaPago < 0)
            {
                return Task.FromResult(false);
            }
            return _comprobanteRepository.CreateComprobanteAsync(comp);
        }

        public Task<bool> BorrarComprobanteAsync(int id)
        {
            if(id < 0)
            {
                return Task.FromResult(false);
            }
            return _comprobanteRepository.DeleteComprobanteAsync(id);
        }
    }
}
