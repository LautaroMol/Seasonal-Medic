namespace APISeasonalMedic.Models
{
    public class CreditCard : EntityBase
    {
        public string Last4Digits { get; set; }
        public string Token { get; set; }
        public string CardType { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string CardId { get; set; }
        public string CustomerId { get; set; }
        public int? PaymentMethodId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
        public User User { get; set; }

    }
}
