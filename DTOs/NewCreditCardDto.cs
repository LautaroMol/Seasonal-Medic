using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class NewCreditCardDto
    {
        public Guid UserId { get; set; }
        public long CardNumber { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string SecurityCode { get; set; }
        public string CardholderName { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public string CardType { get; set; }
    }
}
