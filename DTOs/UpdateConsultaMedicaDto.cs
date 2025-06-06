namespace APISeasonalMedic.DTOs
{
    public class UpdateConsultaMedicaDto
    {
        public DateTime Fecha { get; set; }
        public string Medico { get; set; }
        public string Status { get; set; } // Confirmada, Cancelada, Completada
    }
}
