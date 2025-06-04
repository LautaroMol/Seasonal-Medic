namespace APISeasonalMedic.DTOs
{
    public class CardTokenDto
    {
        public string CardNumber { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string SecurityCode { get; set; }
        public string CardholderName { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public string UserEmail { get; set; }
    }
}
