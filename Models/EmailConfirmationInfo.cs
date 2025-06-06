namespace APISeasonalMedic.Models
{
    public class EmailConfirmationInfo
    {
        public string Email { get; set; }
        public bool CodeSent { get; set; }
        public bool CodeStillValid { get; set; }
        public DateTime? Expiry { get; set; }
    }
}
