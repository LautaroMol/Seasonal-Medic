using System.ComponentModel.DataAnnotations;

namespace APISeasonalTicket.DTOs
{
    public class CreditCardDto
    {
        public string Last4Digits { get; set; }
        public string CardType { get; set; }
        public int ExpirationYear { get; set; }
        public int ExpirationMonth { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string CustomerId { get; set; }
        public string CardId { get; set; }

    }
}
