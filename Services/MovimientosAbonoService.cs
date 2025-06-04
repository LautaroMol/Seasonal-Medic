using APISeasonalTicket.Data;
using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APISeasonalTicket.Services
{
    public class MovimientosAbonoService : IMovimientosAbonoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MovimientosAbonoService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MovimientosAbono> GetMovimientosAbonoByIdAsync(int id)
        {
            var movimientosAbono = await _context.MovimientosAbonos.FindAsync(id);
            return movimientosAbono;
        }

        public async Task<List<MovimientosAbono>> GetAll()
        {
            var movimientosAbonos = await _context.MovimientosAbonos.ToListAsync();
            return movimientosAbonos;
        }
        public async Task<List<MovAbonosDto>> GetByAbonoId(int abonoId)
        {
            var movimientos = await _context.MovimientosAbonos
                .Where(m => m.AbonoId == abonoId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return _mapper.Map<List<MovAbonosDto>>(movimientos);
        }


        public async Task<MovAbonosDto> Post(MovAbonosDto movimientosAbonosDto)
        {
            var movimientosAbono = _mapper.Map<MovimientosAbono>(movimientosAbonosDto);

            // Obtener el abono
            var abono = await _context.Abonos.FindAsync(movimientosAbonosDto.AbonoId);
            if (abono == null)
                throw new Exception("Abono no encontrado");

            if (movimientosAbonosDto.Tipo.ToLower() == "debito" && abono.Total < movimientosAbonosDto.Monto)
                throw new Exception("Fondos insuficientes");

            // Aplicar impacto
            if (movimientosAbonosDto.Tipo.ToLower() == "credito")
                abono.Total += movimientosAbonosDto.Monto;
            else if (movimientosAbonosDto.Tipo.ToLower() == "debito")
                abono.Total -= movimientosAbonosDto.Monto;
            else
                throw new Exception("Tipo de movimiento inválido");

            // Guardar movimiento
            await _context.MovimientosAbonos.AddAsync(movimientosAbono);
            await _context.SaveChangesAsync();

            return _mapper.Map<MovAbonosDto>(movimientosAbono);
        }

        public async Task<MovAbonosDto> Update(MovAbonosDto movimientosAbonosDto, int id)
        {
            var movAbono = await _context.MovimientosAbonos.FindAsync(id);
            if (movAbono == null)
            {
                return null;
            }

            _mapper.Map(movimientosAbonosDto, movAbono);
            _context.MovimientosAbonos.Update(movAbono);
            await _context.SaveChangesAsync();
            return _mapper.Map<MovAbonosDto>(movAbono);
        }

        public async Task<MovAbonosDto> Delete(int id)
        {
            var movimientosAbono = await _context.MovimientosAbonos.FindAsync(id);
            _context.MovimientosAbonos.Remove(movimientosAbono);
            await _context.SaveChangesAsync();
            return _mapper.Map<MovAbonosDto>(movimientosAbono);
        }
    }
}
