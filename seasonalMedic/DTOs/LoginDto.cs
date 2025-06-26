using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Se requiere el email")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Se requiere la contraseña")]
        public string Password { get; set; }
    }
}
