namespace APISeasonalTicket.DTOs
{
    public class CrearSuscripcionDto
    {
        public int UserId { get; set; } = 0;
        public decimal MontoMensual { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public string CardToken { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
