namespace APISeasonalTicket.Models
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; } = 0;
        public string SubscriptionId { get; set; } = string.Empty; 
        public bool Status { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public int CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }
    }
}
