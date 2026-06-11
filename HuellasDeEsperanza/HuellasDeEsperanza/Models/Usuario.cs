using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.Models
{
    public enum TipoVivienda
    {
        Casa,
        Departamento
    }

    public enum RolUsuario
    {
        AdminDesa,   // Solo el desarrollador, invisible
        Admin,       // Invisible al registrarse
        Empleado,    // Invisible al registrarse
        Transito,    // Visible al registrarse
        Adoptante    // Visible al registrarse
    }

    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria")]
        [Range(18, 120, ErrorMessage = "Debe ser mayor de edad para registrarse")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El mail es obligatorio")]
        [EmailAddress(ErrorMessage = "El mail no es válido")]
        public string Mail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Contrasenia { get; set; } = string.Empty;

        // Rol del usuario en el sistema
        public RolUsuario Rol { get; set; } = RolUsuario.Adoptante;

        // Formulario para Adoptante y Tránsito
        public TipoVivienda TipoVivienda { get; set; }

        [Range(0, 20)]
        public int CantidadDeMascotas { get; set; } = 0;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        // Relaciones
        public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();

        // Solicitudes que auditó (como empleado)
        public ICollection<Solicitud> SolicitudesAuditadas { get; set; } = new List<Solicitud>();
    }
}