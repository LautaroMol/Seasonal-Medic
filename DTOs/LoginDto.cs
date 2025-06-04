using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Se requiere el email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Se requiere la contraseña")]
        public string Password { get; set; }
    }
}
