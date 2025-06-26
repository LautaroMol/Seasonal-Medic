namespace APISeasonalMedic.DTOs
{
    public class TransferAbonoDto
    {
        public Guid FromUserId { get; set; } // se saca del token
        public Guid ToUserId { get; set; }
        public decimal Monto { get; set; }
    }
}
