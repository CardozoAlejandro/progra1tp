using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Implementations
{
    public class FuncionRepository : IFuncionRepository
    {
        private readonly CineDbContext _context;
        public FuncionRepository(CineDbContext context)
        {
            _context = context;
        }

        public async Task<Funcion?> GetFuncionByIdAsync(int id)
        {
            return await _context.Funciones
            .AsNoTracking()
            .Include(f => f.Pelicula).ThenInclude(p => p!.Genero)
            .Include(f => f.Pelicula).ThenInclude(p => p!.Clasificacion)
            .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task<int> CountEntradasVendidasAsync(int idFuncion)
        {
            var ahora = DateTime.Now;
            return await _context.ComprobanteDetalles
                .AsNoTracking()
                .CountAsync(d => d.IdFuncion == idFuncion
                              && d.ButacaOcupada
                              && !(d.Comprobante!.Estado == EstadoComprobante.Pendiente
                                   && d.Comprobante.FechaLimiteConfirmacion < ahora));
        }
        public async Task<List<Funcion>> GetFuncionesAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Funciones
                .AsNoTracking()
                .Where(f => f.FechaHoraInicio >= desde && f.FechaHoraInicio < hasta)
                .OrderBy(f => f.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<List<Funcion>> GetFuncionesPeliculasAsync(int idPelicula, DateTime desde)
        {
            return await _context.Funciones
                .AsNoTracking()
                .Where(f => f.IdPelicula == idPelicula && f.FechaHoraInicio >= desde)
                .OrderBy(f => f.FechaHoraInicio)
                .ToListAsync();
        }
        public async Task<List<Butaca>> GetButacasLibresAsync(int idFuncion, int? idSala, int cantidad)
        {
            var ahora = DateTime.Now;

            var ocupadas = _context.ComprobanteDetalles
                .Where(d => d.IdFuncion == idFuncion
                         && d.ButacaOcupada
                         && !(d.Comprobante!.Estado == EstadoComprobante.Pendiente
                              && d.Comprobante.FechaLimiteConfirmacion < ahora))
                .Select(d => d.IdButaca);

            return await _context.Butacas
                .AsNoTracking()
                .Where(b => b.IdSala == idSala
                         && (b.Estado == null || b.Estado == "Habilitada")
                         && !ocupadas.Contains(b.Id))
                .OrderBy(b => b.Fila).ThenBy(b => b.Numero)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
