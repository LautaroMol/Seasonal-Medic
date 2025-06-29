using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class AbonoDto
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
        [Required(ErrorMessage = "Debe ingresar la fecha de creacion")]
        public DateTime CreatedAt { get; set; }
        [Required(ErrorMessage = "Debe ingresar el usuario")]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Debe ingresar el monto mensual")]
        public DateTime LastDebitDate { get; set; }
        public string Plan { get; set; }
        public decimal MontoMensual { get; set; }

    }
}
