namespace APISeasonalMedic.DTOs
{
    public class DebitRequestDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "D�bito puntual desde app";
    }
}
