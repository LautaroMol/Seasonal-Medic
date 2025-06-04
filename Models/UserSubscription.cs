namespace APISeasonalMedic.Models
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string SubscriptionId { get; set; } = string.Empty; 
        public bool Status { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public Guid CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }
    }
}
