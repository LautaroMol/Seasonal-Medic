using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Se requiere el email")]
        [EmailAddress(ErrorMessage = "Formato de email inv�lido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Se requiere la contrase�a")]
        public string Password { get; set; }
    }
}
