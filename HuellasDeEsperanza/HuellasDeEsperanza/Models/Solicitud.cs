using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.Models
{
    public enum TipoSolicitud
    {
        Adopcion,
        Transito,
        Donacion
    }

    public enum EstadoSolicitud
    {
        Pendiente,
        Aceptada,
        Rechazada
    }

    public class Solicitud
    {
        public int Id { get; set; }

        [Required]
        public TipoSolicitud Tipo { get; set; }

        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // La completa el empleado al auditar, deshabilitada para el usuario
        public DateTime? FechaAuditoria { get; set; }

        [StringLength(500)]
        public string? DetalleRechazo { get; set; }

        // Usuario que hace la solicitud
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        // Mascota de la solicitud
        [Required]
        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;

        // Empleado que auditó (puede ser null si todavía no fue auditada)
        public int? AuditadoPorId { get; set; }
        public Usuario? AuditadoPor { get; set; }
    }
}