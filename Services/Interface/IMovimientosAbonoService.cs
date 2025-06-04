using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services.Interface
{
    public interface IMovimientosAbonoService
    {
        Task<MovAbonosDto> Delete(int id);
        Task<List<MovimientosAbono>> GetAll();
        Task<List<MovAbonosDto>> GetByAbonoId(Guid abonoId);
        Task<MovimientosAbono> GetMovimientosAbonoByIdAsync(int id);
        Task<MovAbonosDto> Post(MovAbonosDto movimientosAbonosDto);
        Task<MovAbonosDto> Update(MovAbonosDto movimientosAbonosDto, int id);
    }
}