namespace APISeasonalMedic.Models
{
    public class MovimientosAbono : EntityBase
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string? Descripcion { get; set; }
        public Guid AbonoId { get; set; }
        public Abono Abono { get; set; }
        public string? PaymentId { get; set; }  // ID del pago en MercadoPago
        public string? PaymentStatus { get; set; }  // Estado del pago ("approved", "rejected", etc.)
        public string? PaymentMethod { get; set; }
    }
}
