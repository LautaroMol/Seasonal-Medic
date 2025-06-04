using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public interface IAbonoService
    {
        Task<AbonoDto> Delete(int id);
        Task<Abono> GetAbonoByIdAsync(int id);
        Task<Abono> GetAbonoByUserId(int userId);
        Task<List<Abono>> GetAll();
        Task<AbonoDto> Post(AbonoDto abonodto);
        Task<AbonoDto> Update(AbonoDto abonodto, int id);
        Task<Abono> UpdateDebit(int id, bool debit);
    }
}