using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class CreateAbonoDto
    {
        public decimal Total { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime LastDebitDate { get; set; }

        public string Plan { get; set; }

        public decimal MontoMensual { get; set; }
    }
}
