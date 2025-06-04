using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public interface IMovimientosAbonoService
    {
        Task<MovAbonosDto> Delete(int id);
        Task<List<MovimientosAbono>> GetAll();
        Task<List<MovAbonosDto>> GetByAbonoId(int abonoId);
        Task<MovimientosAbono> GetMovimientosAbonoByIdAsync(int id);
        Task<MovAbonosDto> Post(MovAbonosDto movimientosAbonosDto);
        Task<MovAbonosDto> Update(MovAbonosDto movimientosAbonosDto, int id);
    }
}