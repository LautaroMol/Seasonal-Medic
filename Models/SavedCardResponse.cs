namespace APISeasonalTicket.Models
{
    public class SavedCardResponse
    {
        public string id { get; set; } // CardId en Mercado Pago
        public PaymentMethod payment_method { get; set; }
    }
}
