using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(UserDto userDto);
        Task<UserDto> DeleteUserAsync(int id);
        Task<List<User>> GetAll();
        Task<User> GetUserByDNIAsync(string dni);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(int id);
        Task<UserDto> UpdateUserAsync(UserDto userDto);
        Task<User> UpdateUserAsyncDirect(User user);
        Task<bool> GetMailAvalibility(string email);
    }
}