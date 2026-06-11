using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.Models
{
    public enum TipoMascota
    {
        Perro,
        Gato
    }

    public enum Tamanio
    {
        Chico,
        Mediano,
        Grande
    }

    public enum EstadoMedico
    {
        AmbulatorioConBaja,
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

        [Required]
        public EstadoMedico Estado { get; set; } = EstadoMedico.AmbulatorioConBaja;

        public bool EstaVacunada { get; set; } = false;
        public bool EstaCastrada { get; set; } = false;
        public bool EstaDesparasitada { get; set; } = false;

        // Disponible se calcula: no puede estar disponible si tiene AmbulatorioConBaja
        public bool EstaDisponible { get; set; } = false;

        // Relación con Usuario dueño (null si no tiene dueño)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Solicitudes asociadas a esta mascota
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    }
}