using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class UpdateAbonoDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public DateTime LastDebitDate { get; set; }

        public string Plan { get; set; }

        [Required]
        public decimal MontoMensual { get; set; }
    }
}
