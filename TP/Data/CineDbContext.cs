using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cine_TP.Data
{
    public class CineDbContext : DbContext
    {
        public CineDbContext(DbContextOptions<CineDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ComprobanteDetalle> ComprobanteDetalles { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Funcion> Funciones { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<TipoSala> TiposSala { get; set; }
        public DbSet<Butaca> Butacas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Clasificacion> Clasificaciones { get; set; }
        public DbSet<TipoPublico> TiposPublico { get; set; }
        public DbSet<FormaPago> FormasPago { get; set; }
        public DbSet<Promocion> Promociones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ComprobanteDetalle>()
                .HasOne(d => d.Comprobante)
                .WithMany(c => c.ComprobanteDetalles)
                .HasForeignKey(d => d.IdComprobante)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Dni)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<ComprobanteDetalle>()
                .HasIndex(d => new { d.IdFuncion, d.IdButaca })
                .IsUnique()
                .HasFilter("[butaca_ocupada] = 1");

            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys())
                         .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade
                                      && fk.PrincipalEntityType.ClrType != typeof(Comprobante)))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}