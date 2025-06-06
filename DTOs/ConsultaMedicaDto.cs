namespace APISeasonalMedic.DTOs
{
    public class ConsultaMedicaDto
    {
        public Guid Id { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Descripcion { get; set; }
        public string? Especialidad { get; set; }
        public string? Medico { get; set; }
        public string Status { get; set; }

        // Info de usuario para agentes
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string DNI { get; set; }
    }
}
