namespace APISeasonalTicket.DTOs
{
    public class MakePaymentDto
    {
        public int UserId { get; set; }
        public int CardId { get; set; }
        public string SecurityCode { get; set; }
        public decimal Amount { get; set; }
    }
}
