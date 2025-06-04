using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APISeasonalMedic.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;


        public CreditCardService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CreditCard>> GetAll()
        {
            return await _context.CreditCards.ToListAsync();
        }

        public async Task<CreditCard> GetCreditCardByIdAsync(int id)
        {
            var creditCard = await _context.CreditCards.FindAsync(id);
            if (creditCard == null)
            {
                return null;
            }
            return creditCard;
        }
        public async Task<List<CreditCard>> GetCreditCardsByUserIdAsync(Guid userId)
        {
            return await _context.CreditCards
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        public async Task<CreditCard> GetActiveCreditCardByUserIdAsync(Guid userId)
        {
            return await _context.CreditCards
                .Where(c => c.UserId == userId && c.IsPrimary) // Filtrar por tarjeta activa
                .FirstOrDefaultAsync();
        }

        public async Task<CreditCardDto> CreateCreditCardAsync(CreditCardDto creditCardDto)
        {
            var creditCard = _mapper.Map<CreditCard>(creditCardDto);
            await _context.CreditCards.AddAsync(creditCard);
            await _context.SaveChangesAsync();
            return _mapper.Map<CreditCardDto>(creditCard);
        }
        public async Task<bool> SetMainCardAsync(Guid userId, Guid cardId)
        {
            var cards = await _context.CreditCards
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cards.Any())
                return false;

            var targetCard = cards.FirstOrDefault(c => c.Id == cardId);
            if (targetCard == null)
                return false;

            foreach (var card in cards)
            {
                card.IsPrimary = card.Id == cardId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CreditCard> AddCreditCardAsync(CreditCard creditCard)
        {
            await _context.CreditCards.AddAsync(creditCard);
            await _context.SaveChangesAsync();
            return creditCard;
        }
        public async Task<CreditCardDto> UpdateCreditCardAsync(CreditCardDto creditCardDto, int id)
        {
            var creditCard = await _context.CreditCards.FindAsync(id);

            if (creditCard == null)
                throw new Exception("Tarjeta no encontrada");
            _mapper.Map(creditCardDto, creditCard);
            _context.CreditCards.Update(creditCard);
            await _context.SaveChangesAsync();
            return _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task<CreditCardDto> DeleteCreditCardAsync(int id)
        {
            var creditCard = await _context.CreditCards.FindAsync(id);

            if (creditCard == null)
                throw new InvalidOperationException("La tarjeta no existe.");

            // Obtener cuántas tarjetas tiene el usuario
            var totalCards = await _context.CreditCards
                .CountAsync(c => c.UserId == creditCard.UserId);

            if (creditCard.IsPrimary)
                throw new InvalidOperationException("No se puede eliminar la tarjeta principal.");

            if (totalCards == 1)
                throw new InvalidOperationException("No se puede eliminar la única tarjeta registrada.");

            _context.CreditCards.Remove(creditCard);
            await _context.SaveChangesAsync();

            return _mapper.Map<CreditCardDto>(creditCard);
        }


    }
}
