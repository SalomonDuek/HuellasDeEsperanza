using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.ViewModels
{
    public class RegisterViewModel
    {
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

        [Required(ErrorMessage = "Confirmá la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Contrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasenia { get; set; } = string.Empty;

        // Datos de vivienda
        public HuellasDeEsperanza.Models.TipoVivienda TipoVivienda { get; set; }

        [Range(0, 15, ErrorMessage = "Máximo 15 mascotas")]
        public int CantidadDeMascotas { get; set; } = 0;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;
    }
}