using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Models;

namespace HuellasDeEsperanza.Data
{
    public class HDEDbContext : DbContext
    {
        public HDEDbContext(DbContextOptions<HDEDbContext> options) : base(options)
        {
        }

        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mascota → Usuario (dueño): relación opcional, no en cascada
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Solicitud → Usuario (solicitante)
            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Solicitudes)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Solicitud → Mascota
            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Mascota)
                .WithMany(m => m.Solicitudes)
                .HasForeignKey(s => s.MascotaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Solicitud → Usuario (auditor/empleado): relación opcional
            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.AuditadoPor)
                .WithMany(u => u.SolicitudesAuditadas)
                .HasForeignKey(s => s.AuditadoPorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}