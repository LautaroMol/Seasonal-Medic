using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Se requiere el apellido")]
        [MaxLength(255)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Se requiere el DNI")]
        [MaxLength(10)]
        public string DNI { get; set; }

        [Required(ErrorMessage = "Se requiere el n�mero de tel�fono")]
        [MaxLength(14)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Se requiere el email")]
        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "Formato de email inv�lido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Se requiere la contrase�a")]
        [MinLength(6, ErrorMessage = "La contrase�a debe tener al menos 6 caracteres")]
        [MaxLength(50)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Se requiere confirmar la contrase�a")]
        [Compare("Password", ErrorMessage = "Las contrase�as no coinciden")]
        public string ConfirmPassword { get; set; }

        public string? AreaCode { get; set; }
        public string? ProfileImage { get; set; }
    }
}
