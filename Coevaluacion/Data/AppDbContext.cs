using Microsoft.EntityFrameworkCore;
using Coevaluacion.Models;

namespace Coevaluacion.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Integrante> Integrantes { get; set; }
        public DbSet<Criterio> Criterios { get; set; }
        public DbSet<Periodo> Periodos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relaciones (Code First)
            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.Integrantes)
                .WithOne(i => i.Equipo)
                .HasForeignKey(i => i.EquipoId);
        }
    }
}
