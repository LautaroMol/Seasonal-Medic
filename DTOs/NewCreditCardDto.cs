using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.DTOs
{
    public class NewCreditCardDto
    {
        [Required]
        public string CardNumber { get; set; }

        [Required]
        public int ExpirationMonth { get; set; }

        [Required]
        public int ExpirationYear { get; set; }

        [Required]
        public string SecurityCode { get; set; }

        [Required]
        public string CardholderName { get; set; }

        [Required]
        public string IdentificationType { get; set; }

        [Required]
        public string IdentificationNumber { get; set; }
        [Required]
        public string CardType { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
