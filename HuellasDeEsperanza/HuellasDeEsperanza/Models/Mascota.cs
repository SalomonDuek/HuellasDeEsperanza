using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.Models
{
    public enum TipoMascota
    {
        Perro,
        Gato
    }

    public enum Disponibilidad
    {
        Adoptable,
        Transitable
    }

    public enum Tamanio
    {
        Chico,
        Mediano,
        Grande
    }

    public enum EstadoMedico
    {
        EnTratamiento,
        AltaMedica
    }

    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        public string Imagen { get; set; } = string.Empty;

        [Required]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de mascota es obligatorio")]
        public TipoMascota TipoMascota { get; set; }

        [Required(ErrorMessage = "El tamaño es obligatorio")]
        public Tamanio Tamanio { get; set; }

        [Required(ErrorMessage = "El estado médico es obligatorio")]
        public EstadoMedico Estado { get; set; }

        // Se calcula automáticamente según el Estado
        public Disponibilidad Disponibilidad { get; set; }

        // Sanitarios
        public bool EstaVacunada { get; set; } = false;

        public bool EstaCastrada { get; set; } = false;

        public bool EstaDesparasitada { get; set; } = false;

        // Disponibilidad general
        public bool EstaDisponible { get; set; } = true;

        // Control de adopción/tránsito
        public bool Adoptado { get; set; } = false;

        public bool Transitado { get; set; } = false;

        // Relación con usuario
        public int? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        // Solicitudes asociadas
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    }
}