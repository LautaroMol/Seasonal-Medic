using System.Security.Cryptography.X509Certificates;

namespace APISeasonalTicket.Models
{
    public class Abono : EntityBase
    {
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal MontoMensual { get; set; }
        public string Plan { get; set; } 
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime LastDebitDate { get; set; }
        public bool Debit { get; set; } = true;
        public string? SubscriptionId { get; set; }
        public virtual ICollection<MovimientosAbono> Movimientos { get; set; }
    }
}
