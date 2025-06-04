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

        [Required(ErrorMessage = "Se requiere el número de teléfono")]
        [MaxLength(14)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Se requiere el email")]
        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Se requiere la contraseña")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [MaxLength(50)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Se requiere confirmar la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        public string? AreaCode { get; set; }
        public string? ProfileImage { get; set; }
    }
}
