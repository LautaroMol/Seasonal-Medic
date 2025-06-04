using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services.Interface
{
    public interface IAbonoService
    {
        Task<AbonoDto> Delete(int id);
        Task<Abono> GetAbonoByIdAsync(int id);
        Task<Abono> GetAbonoByUserId(Guid userId);
        Task<List<Abono>> GetAll();
        Task<AbonoDto> Post(AbonoDto abonodto);
        Task<AbonoDto> Update(AbonoDto abonodto, int id);
        Task<Abono> UpdateDebit(int id, bool debit);
    }
}