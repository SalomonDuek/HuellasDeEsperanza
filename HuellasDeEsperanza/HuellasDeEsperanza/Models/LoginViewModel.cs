using System.ComponentModel.DataAnnotations;
using HuellasDeEsperanza.Models;

namespace HuellasDeEsperanza.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El mail es obligatorio")]
        [EmailAddress(ErrorMessage = "Mail inválido")]
        public string Mail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria")]
        [Range(18, 120, ErrorMessage = "Debe ser mayor de edad")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El mail es obligatorio")]
        [EmailAddress(ErrorMessage = "Mail inválido")]
        public string Mail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public string Contrasenia { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Contrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasenia { get; set; } = string.Empty;

        // Solo Tránsito y Adoptante visibles al registrarse
        [Required(ErrorMessage = "Seleccioná un rol")]
        public RolUsuario Rol { get; set; } = RolUsuario.Adoptante;

        public TipoVivienda TipoVivienda { get; set; }

        [Range(0, 20)]
        public int CantidadDeMascotas { get; set; } = 0;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;
    }
}