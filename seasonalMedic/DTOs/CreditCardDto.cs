using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class CreditCardDto
    {
        public string Last4Digits { get; set; }
        public string CardType { get; set; }
        public int ExpirationYear { get; set; }
        public int ExpirationMonth { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string CustomerId { get; set; }
        public string CardId { get; set; }

    }
}
