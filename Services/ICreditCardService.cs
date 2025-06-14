using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services
{
    public interface ICreditCardService
    {
        Task<CreditCard> AddCreditCardAsync(CreditCard creditCard);
        Task<CreditCardDto> CreateCreditCardAsync(CreditCardDto creditCardDto);
        Task<CreditCardDto> DeleteCreditCardAsync(Guid id);
        Task<CreditCard> GetActiveCreditCardByUserIdAsync(Guid userId);
        Task<List<CreditCard>> GetAll();
        Task<CreditCard> GetCreditCardByIdAsync(int id);
        Task<List<CreditCard>> GetCreditCardsByUserIdAsync(Guid userId);
        Task<bool> SetMainCardAsync(Guid userId, Guid cardId);
        Task<CreditCardDto> UpdateCreditCardAsync(CreditCardDto creditCardDto, int id);
    }
}