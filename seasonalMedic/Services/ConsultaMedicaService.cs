using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APISeasonalMedic.Services
{
    public class ConsultaMedicaService : IConsultaMedicaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ConsultaMedicaService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ConsultaMedicaDto> CreateAsync(CreateConsultaMedicaDto dto, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            var consulta = _mapper.Map<ConsultaMedica>(dto);
            consulta.UserId = userId;
            consulta.Usuario = user;

            _context.ConsultasMedicas.Add(consulta);
            await _context.SaveChangesAsync();

            return _mapper.Map<ConsultaMedicaDto>(consulta);
        }
        public async Task<bool> UpdateAsync(Guid consultaId, UpdateConsultaMedicaDto dto)
        {
            var consulta = await _context.ConsultasMedicas.FindAsync(consultaId);
            if (consulta == null) return false;

            _mapper.Map(dto, consulta);

            _context.ConsultasMedicas.Update(consulta);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<ConsultaMedicaDto>> GetAllAsync()
        {
            var consultas = await _context.ConsultasMedicas.Include(c => c.Usuario).ToListAsync();
            return _mapper.Map<List<ConsultaMedicaDto>>(consultas);
        }

        public async Task<List<ConsultaMedicaDto>> GetByDniAsync(string dni)
        {
            var consultas = await _context.ConsultasMedicas
                .Include(c => c.Usuario)
                .Where(c => c.Usuario.DNI == dni)
                .ToListAsync();

            return _mapper.Map<List<ConsultaMedicaDto>>(consultas);
        }

        public async Task<List<ConsultaMedicaDto>> GetByUserIdAsync(Guid userId)
        {
            var consultas = await _context.ConsultasMedicas
                .Include(c => c.Usuario)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<ConsultaMedicaDto>>(consultas);
        }

    }
}
