namespace APISeasonalTicket.DTOs
{
    public class DebitRequestDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "Débito puntual desde app";
    }
}
