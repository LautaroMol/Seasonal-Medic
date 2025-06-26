namespace APISeasonalMedic.Models
{
    public class AutoRecurring
    {
        public int Frequency { get; set; } = 1;
        public string FrequencyType { get; set; } = "months";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string CurrencyId { get; set; } = "ARS";
    }
}
