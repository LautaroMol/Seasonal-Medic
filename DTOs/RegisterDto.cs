using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Se requiere el nombre completo")]
        [MaxLength(255)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Se requiere el nombre completo")]
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
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Se requiere la contraseña")]
        [MaxLength(50)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Se requiere la contraseña")]
        [MaxLength(50)]
        public string ConfirmPassword { get; set; }
        public string AreaCode { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
