using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Data;

namespace ViajesRepository.Data.Implementations
{
    public class ComprobanteRepository : IComprobanteRepository
    {
        private readonly CineDbContext _context;

        public ComprobanteRepository(CineDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comprobante>> GetAllComprobantesAsync()
        {
            return await _context.Comprobantes
                                 .Include(c => c.ComprobanteDetalles)
                                 .ToListAsync();
        }

        public async Task<Comprobante?> GetComprobanteByIdAsync(int id)
        {
            return await _context.Comprobantes
                                 .Include(c => c.ComprobanteDetalles)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CreateComprobanteAsync(Comprobante comprobante)
        {
            // Calculamos subtotales y total iterando los detalles
            decimal totalViaje = 0;

            comprobante.MontoTotal = totalViaje;

            // agregamos el Viaje y se mapean automáticamente sus ViajeDetalles
            await _context.Comprobantes.AddAsync(comprobante);

            // SaveChangesAsync ejecuta TODO en una sola transacción
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateEstadoAsync(int id, string nuevoEstado)
        {
            var comprobante = await _context.Comprobantes.FindAsync(id);
            if (comprobante == null)
                return false;

            comprobante.Estado = nuevoEstado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteComprobanteAsync(int id)
        {
            var c = await _context.Comprobantes.FindAsync(id);
            if (c == null)
                return false;

            _context.Comprobantes.Remove(c);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
