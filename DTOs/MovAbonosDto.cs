namespace APISeasonalTicket.DTOs
{
    public class MovAbonosDto
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string? Descripcion { get; set; }
        public int AbonoId { get; set; }
    }
}
