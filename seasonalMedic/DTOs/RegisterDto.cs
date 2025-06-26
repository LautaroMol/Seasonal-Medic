using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APISeasonalMedic.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Se requiere el apellido")]
        [MaxLength(255)]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Se requiere el DNI")]
        [MaxLength(10)]
        [JsonPropertyName("dni")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "Se requiere el número de teléfono")]
        [MaxLength(16)]
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Se requiere el email")]
        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Se requiere la contraseña")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [MaxLength(50)]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Se requiere confirmar la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [JsonPropertyName("areaCode")]
        public string? AreaCode { get; set; }

        [JsonPropertyName("profileImage")]
        public string? ProfileImage { get; set; }
    }
}