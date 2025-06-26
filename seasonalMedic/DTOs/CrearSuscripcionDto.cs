namespace APISeasonalMedic.DTOs
{
    public class CrearSuscripcionDto
    {
        public Guid UserId { get; set; }
        public decimal MontoMensual { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public string CardToken { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
