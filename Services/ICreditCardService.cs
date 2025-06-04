using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public interface ICreditCardService
    {
        Task<CreditCard> AddCreditCardAsync(CreditCard creditCard);
        Task<CreditCardDto> CreateCreditCardAsync(CreditCardDto creditCardDto);
        Task<CreditCardDto> DeleteCreditCardAsync(int id);
        Task<CreditCard> GetActiveCreditCardByUserIdAsync(int userId);
        Task<List<CreditCard>> GetAll();
        Task<CreditCard> GetCreditCardByIdAsync(int id);
        Task<List<CreditCard>> GetCreditCardsByUserIdAsync(int userId);
        Task<bool> SetMainCardAsync(int userId, int cardId);
        Task<CreditCardDto> UpdateCreditCardAsync(CreditCardDto creditCardDto, int id);
    }
}