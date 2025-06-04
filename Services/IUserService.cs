using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services
{
    public interface IUserService
    {
        Task<bool> DeleteUserAsync(Guid userId);
        Task<UserDto> GetCurrentUserDtoAsync();
        Task<User> GetCurrentUserEntityAsync();
        Task<bool> GetMailAvailabilityAsync(string email);
        Task<User> GetUserByEmailAsync(string email);
        Task<UserDto> GetUserDtoByIdAsync(Guid id);
        Task<User> GetUserEntityByIdAsync(Guid id);
        Task<LoginResponseDto> LoginAsync(LoginDTO dto);
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<bool> UpdateUserAsyncDirect(User user);
        Task<UserDto> UpdateUserDtoAsync(UserDto dto);
        Task<User> UpdateUserEntityAsync(User user);
    }
}