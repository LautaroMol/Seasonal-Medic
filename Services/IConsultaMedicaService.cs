using APISeasonalMedic.DTOs;

namespace APISeasonalMedic.Services
{
    public interface IConsultaMedicaService
    {
        Task<ConsultaMedicaDto> CreateAsync(CreateConsultaMedicaDto dto, Guid userId);
        Task<List<ConsultaMedicaDto>> GetAllAsync();
        Task<List<ConsultaMedicaDto>> GetByDniAsync(string dni);
        Task<List<ConsultaMedicaDto>> GetByUserIdAsync(Guid userId);
        Task<bool> UpdateAsync(Guid consultaId, UpdateConsultaMedicaDto dto);
    }
}