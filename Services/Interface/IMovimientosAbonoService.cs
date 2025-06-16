using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services.Interface
{
    public interface IMovimientosAbonoService
    {
        Task<MovAbonosDto> Delete(Guid id);
        Task<List<MovimientosAbono>> GetAll();
        Task<List<MovAbonosDto>> GetByAbonoId(Guid abonoId);
        Task<List<MovAbonosDto>> GetMovimientosByUserId(Guid userId);
        Task<MovimientosAbono> GetMovimientosAbonoByIdAsync(Guid id);
        Task<MovAbonosDto> Post(MovAbonosDto movimientosAbonosDto);
        Task<MovAbonosDto> Update(MovAbonosDto movimientosAbonosDto, Guid id);
    }
}