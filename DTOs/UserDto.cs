using APISeasonalTicket.Models;
using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string DNI { get; set; }
        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Se requiere el nombre Apellido")]
        [MaxLength(255)]
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNumber { get; set; }
        public string? CustomerId { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
