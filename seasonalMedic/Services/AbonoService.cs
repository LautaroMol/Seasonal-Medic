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

        public async Task<Abono> GetAbonoByIdAsync(Guid id)
        {
            return await _context.Abonos.FindAsync(id);
        }

        public async Task<List<Abono>> GetAll()
        {
            return await _context.Abonos.ToListAsync();
        }

        public async Task<Abono> GetAbonoByUserId(Guid userId)
        {
            return await _context.Abonos
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
        public async Task<Abono> GetAbonoByDniAsync(string dni)
        {
            var abono = await (from a in _context.Abonos
                               join u in _context.Users on a.UserId equals u.Id
                               where u.DNI == dni
                               select a).FirstOrDefaultAsync();

            return abono;
        }

        public async Task<AbonoDto> Post(CreateAbonoDto dto, Guid userId)
        {
            var abono = _mapper.Map<Abono>(dto);
            abono.UserId = userId;
            abono.LastDebitDate = dto.LastDebitDate.AddMonths(1);
            await _context.Abonos.AddAsync(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }

        public async Task<AbonoDto> Update(UpdateAbonoDto dto)
        {
            var abono = await _context.Abonos.FindAsync(dto.Id);
            if (abono == null)
                throw new KeyNotFoundException("Abono no encontrado");

            // Mapea solo propiedades editables
            abono.Total = dto.Total;
            abono.Plan = dto.Plan;
            abono.MontoMensual = dto.MontoMensual;
            abono.LastDebitDate = dto.LastDebitDate;

            _context.Abonos.Update(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }

        public async Task Delete(Guid id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null)
                throw new KeyNotFoundException("Abono no encontrado");

            _context.Abonos.Remove(abono);
            await _context.SaveChangesAsync();
        }

        public async Task<AbonoDto> UpdateDebit(Guid id, bool debit)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null)
                throw new KeyNotFoundException("Abono no encontrado");

            abono.Debit = debit;
            _context.Abonos.Update(abono);
            await _context.SaveChangesAsync();
            return _mapper.Map<AbonoDto>(abono);
        }
        public async Task TransferirAsync(Guid fromUserId, Guid toUserId, decimal monto)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a cero");

            var abonoOrigen = await _context.Abonos.FirstOrDefaultAsync(a => a.UserId == fromUserId);
            var abonoDestino = await _context.Abonos.FirstOrDefaultAsync(a => a.UserId == toUserId);

            if (abonoOrigen == null || abonoDestino == null)
                throw new KeyNotFoundException("No se encontró alguno de los abonos");

            if (abonoOrigen.Total < monto)
                throw new InvalidOperationException("Saldo insuficiente");

            abonoOrigen.Total -= monto;
            abonoDestino.Total += monto;

            // Opcional: registrar movimientos
            _context.MovimientosAbonos.AddRange(new List<MovimientosAbono>
    {
        new MovimientosAbono
        {
            AbonoId = abonoOrigen.Id,
            Monto = -monto,
            Fecha = DateTime.Now,
            Tipo = "Transferencia enviada",
            Descripcion = $"Transferencia a {abonoDestino.UserId}"
        },
        new MovimientosAbono
        {
            AbonoId = abonoDestino.Id,
            Monto = monto,
            Fecha = DateTime.Now,
            Tipo = "Transferencia recibida",
            Descripcion = $"Transferencia de {abonoOrigen.UserId}"
        }
    });

            await _context.SaveChangesAsync();
        }
    }
}
