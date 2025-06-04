using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.Models
{
    public class User : IdentityUser<int>
    {
        [Required(ErrorMessage = "Se requiere el nombre")]
        [MaxLength(255)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Se requiere el nombre Apellido")]
        [MaxLength(255)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Se requiere el DNI")]
        [MaxLength(9)]
        public string DNI { get; set; }
        [Required(ErrorMessage = "Se requiere el número de teléfono")]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }
        public string? AreaCode { get; set; }
        public string? VerificationCode { get; set; }
        public string? CustomerId { get; set; }
        public string? CardToken { get; set; }
        public string? ProfileImageUrl { get; set; }

        public virtual ICollection<CreditCard> Cards { get; set; }
        public virtual Abono Abono { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
