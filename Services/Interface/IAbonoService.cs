using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services.Interface
{
    public interface IAbonoService
    {
        Task Delete(Guid id);
        Task<Abono> GetAbonoByDniAsync(string dni);
        Task<Abono> GetAbonoByIdAsync(Guid id);
        Task<Abono> GetAbonoByUserId(Guid userId);
        Task<List<Abono>> GetAll();
        Task<AbonoDto> Post(CreateAbonoDto dto, Guid userId);
        Task TransferirAsync(Guid fromUserId, Guid toUserId, decimal monto);
        Task<AbonoDto> Update(UpdateAbonoDto dto);
        Task<AbonoDto> UpdateDebit(Guid id, bool debit);
    }
}