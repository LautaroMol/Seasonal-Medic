namespace APISeasonalMedic.DTOs
{
    public class MakePaymentDto
    {
        public Guid UserId { get; set; }
        public int CardId { get; set; }
        public string SecurityCode { get; set; }
        public decimal Amount { get; set; }
    }
}
