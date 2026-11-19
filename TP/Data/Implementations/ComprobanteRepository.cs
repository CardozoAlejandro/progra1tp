using Cine_TP.Data;
using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Implementations
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
                                 .Where(c => c.Id == id)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateComprobanteAsync(Comprobante comprobante)
        {
            var detalles = comprobante.ComprobanteDetalles;
            bool repetidas = detalles
                             .GroupBy(d => new { d.IdFuncion, d.IdButaca })
                             .Any(g => g.Count() > 1);
            if (repetidas)
                throw new InvalidOperationException("Hay butacas repetidas en el comprobante.");
            await using var tx = await _context.Database.BeginTransactionAsync();

            comprobante.Id = 0;
            comprobante.Cliente = null;
            comprobante.FormaPago = null;
            comprobante.Promocion = null;
            var ahora = DateTime.Now;
            foreach (var d in detalles)
            {
                d.Id = 0;
                d.Comprobante = null;
                d.Funcion = null;
                d.Butaca = null;
                d.TipoPublico = null;

                var vencidos = await _context.ComprobanteDetalles
                    .Where(x => x.IdFuncion == d.IdFuncion
                             && x.IdButaca == d.IdButaca
                             && x.ButacaOcupada
                             && x.Comprobante!.Estado == EstadoComprobante.Pendiente
                             && x.Comprobante.FechaLimiteConfirmacion < ahora)
                    .ToListAsync();

                foreach (var v in vencidos)
                    v.ButacaOcupada = false;

                if (vencidos.Count > 0)
                    await _context.SaveChangesAsync();

                bool ocupada = await _context.ComprobanteDetalles.AnyAsync(x =>
                    x.IdFuncion == d.IdFuncion &&
                    x.IdButaca == d.IdButaca &&
                    x.ButacaOcupada);

                if (ocupada)
                    throw new InvalidOperationException(
                        $"La butaca {d.IdButaca} ya está ocupada para la función {d.IdFuncion}.");

                d.ButacaOcupada = true;
            }

            comprobante.MontoTotal = detalles.Sum(d => d.PrecioBase);

            decimal descuento = 0;
            if (comprobante.IdPromocion is not null)
            {
                var promo = await _context.Promociones
                    .AsNoTracking()
                    .Where(p => p.Id == comprobante.IdPromocion)
                    .FirstOrDefaultAsync();

                if (promo == null)
                    throw new InvalidOperationException("La promoción indicada no existe.");

                ValidarPromocion(promo, ahora);

                descuento = Math.Round(comprobante.MontoTotal * promo.PorcentajeDescuento / 100m, 2);
            }

            comprobante.MontoFinal = comprobante.MontoTotal - descuento;

            await _context.Comprobantes.AddAsync(comprobante);
            bool ok = await _context.SaveChangesAsync() > 0;

            await tx.CommitAsync();
            return ok;
        }

        public async Task<bool> UpdateEstadoAsync(int id, string nuevoEstado)
        {
            var comprobante = await _context.Comprobantes
                    .Include(c => c.ComprobanteDetalles)
                    .FirstOrDefaultAsync(c => c.Id == id);
            if (comprobante == null)
                return false;

            comprobante.Estado = nuevoEstado;
            bool ocupada = nuevoEstado != EstadoComprobante.Cancelado;
            foreach (var d in comprobante.ComprobanteDetalles)
                d.ButacaOcupada = ocupada;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteComprobanteAsync(int id)
        {
            var c = await _context.Comprobantes
                                  .Include(c => c.ComprobanteDetalles)
                                  .Where(c => c.Id == id)
                                  .FirstOrDefaultAsync();
            if (c == null)
                return false;

            _context.ComprobanteDetalles.RemoveRange(c.ComprobanteDetalles);
            _context.Comprobantes.Remove(c);
            return await _context.SaveChangesAsync() > 0;
        }

        private static void ValidarPromocion(Promocion promo, DateTime referencia)
        {
            var cultura = new CultureInfo("es-AR");

            if (!string.IsNullOrWhiteSpace(promo.DiaSemana))
            {
                string dia = referencia.ToString("dddd", cultura);
                if (string.Compare(dia, promo.DiaSemana.Trim(), cultura,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) != 0)
                    throw new InvalidOperationException(
                        $"La promoción '{promo.NombrePromo}' solo es válida los {promo.DiaSemana}.");
            }

            if (promo.HoraInicio is not null && promo.HoraFin is not null)
            {
                var hora = referencia.TimeOfDay;
                if (hora < promo.HoraInicio || hora > promo.HoraFin)
                    throw new InvalidOperationException(
                        $"La promoción '{promo.NombrePromo}' solo es válida de {promo.HoraInicio:hh\\:mm} a {promo.HoraFin:hh\\:mm}.");
            }
        }

        public async Task<List<(string pelicula, decimal total)>> GetIngresosAsync()
        {
            var datos = await _context.ComprobanteDetalles
            .AsNoTracking()
            .Where(d => d.Comprobante!.Estado == EstadoComprobante.Confirmada
                 && d.Comprobante.MontoTotal > 0)
            .GroupBy(d => new { d.Funcion!.IdPelicula, d.Funcion.Pelicula!.Titulo })
            .Select(g => new
            {
                Pelicula = g.Key.Titulo,
                Total = g.Sum(d => d.PrecioBase * d.Comprobante!.MontoFinal / d.Comprobante.MontoTotal)
            })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

            return datos.Select(x => (x.Pelicula, x.Total)).ToList();
        }
        
        public async Task<List<(int anio, int mes, decimal total)>> GetIngresosMensualesAsync()
        {
            var datos = await _context.Comprobantes
            .AsNoTracking()
            .Where(c => c.Estado == EstadoComprobante.Confirmada)
            .Select(c => new { Fecha = c.FechaHoraEmision ?? c.FechaHoraCreacion, c.MontoFinal })
            .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(x => x.MontoFinal)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

            return datos.Select(x => (x.Year, x.Month, x.Total)).ToList();
        }

        public async Task<int?> GetIdClienteAsync(int? idUsuario, string? nombreUsuario)
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => idUsuario != null
                            ? c.IdUsuario == idUsuario
                            : c.Usuario!.NombreUsuario == nombreUsuario)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> FormaPagoExistsAsync(int id)
        {
            return await _context.FormasPago.AsNoTracking().AnyAsync(f => f.Id == id);
        }
    }
}
