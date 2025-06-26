namespace APISeasonalMedic.Models
{
    public class ConsultaMedica : EntityBase
    {
        public DateTime? Fecha { get; set; }
        public string? Descripcion { get; set; } = "Sin especificar";
        public string? Especialidad { get; set; } = " Sin especificar";
        public string? Medico { get; set; }
        public string Status { get; set; } = "Pendiente"; // Valores posibles: Pendiente, Confirmada, Cancelada, Completada
        public Guid UserId { get; set; }
        public User Usuario { get; set; }
    }
}
