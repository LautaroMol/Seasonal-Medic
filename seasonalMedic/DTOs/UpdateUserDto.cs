using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Se requiere el apellido")]
        [MaxLength(255)]
        public string LastName { get; set; }

        public string? AreaCode { get; set; }

        [Required(ErrorMessage = "Se requiere el número de teléfono")]
        [MaxLength(14)]
        public string PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }
    }
}
