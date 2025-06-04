using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APISeasonalMedic.Services
{
    public class AbonoService : IAbonoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AbonoService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Abono> GetAbonoByIdAsync(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            return abono;
        }
        public async Task<List<Abono>> GetAll()
        {
            var abonos = await _context.Abonos.ToListAsync();
            return abonos;
        }
        public async Task<Abono> GetAbonoByUserId(Guid userId)
        {
            var abono = await _context.Abonos
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();
            return abono;
        }
        public async Task<AbonoDto> Post(AbonoDto abonodto)
        {
            var abono = _mapper.Map<Abono>(abonodto);
            abono.LastDebitDate = abonodto.LastDebitDate.AddMonths(1);
            await _context.Abonos.AddAsync(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }
        public async Task<AbonoDto> Update(AbonoDto abonodto, int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null)
            {
                throw new Exception("Abono no encontrado");
            }

            _mapper.Map(abonodto, abono);
            _context.Abonos.Update(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }
        public async Task<AbonoDto> Delete(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            _context.Abonos.Remove(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }
        public async Task<Abono> UpdateDebit(int id, bool debit)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null)
                return null;

            abono.Debit = debit;
            _context.Abonos.Update(abono);
            await _context.SaveChangesAsync();

            return abono;
        }
    }
}
