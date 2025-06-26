using APISeasonalMedic.Models;
using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string DNI { get; set; }

        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Se requiere el apellido")]
        [MaxLength(255)]
        public string LastName { get; set; }

        public string Email { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNumber { get; set; }
        public string? CustomerId { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
