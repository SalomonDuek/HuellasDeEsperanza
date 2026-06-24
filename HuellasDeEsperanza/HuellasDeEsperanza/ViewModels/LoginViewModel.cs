using System.ComponentModel.DataAnnotations;

namespace HuellasDeEsperanza.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El mail es obligatorio")]
        [EmailAddress(ErrorMessage = "El mail no es válido")]
        public string Mail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;
    }
}