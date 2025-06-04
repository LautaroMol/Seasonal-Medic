using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace APISeasonalMedic.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;

        public UserService(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
        }

        private Guid GetUserIdFromToken()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId ?? throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario."));
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;
            user.Email = dto.Email;

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Error al registrar el usuario.");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Credenciales inválidas");
            var roles = await _userManager.GetRolesAsync(user);

            var result = _jwtService.GenerateToken(user.Id, user.Email, roles.ToList());
            return new LoginResponseDto { Token = result.Token, ExpiresAt = result.ExpiresAt };

        }

        public async Task<UserDto> GetCurrentUserDtoAsync()
        {
            var user = await GetCurrentUserEntityAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<User> GetCurrentUserEntityAsync()
        {
            var userId = GetUserIdFromToken();
            var user = await _context.Users.FindAsync(userId);
            return user ?? throw new KeyNotFoundException("Usuario no encontrado");
        }
        public async Task<User> GetUserEntityByIdAsync(Guid id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<UserDto> GetUserDtoByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId)
                       ?? throw new KeyNotFoundException("Usuario no encontrado");
            return _mapper.Map<UserDto>(user);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> UpdateUserAsyncDirect(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        public async Task<bool> GetMailAvailabilityAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserDto> UpdateUserDtoAsync(UserDto dto)
        {
            var userId = GetUserIdFromToken();
            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null) throw new KeyNotFoundException("Usuario no encontrado");

            _mapper.Map(dto, existingUser);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(existingUser);
        }

        public async Task<User> UpdateUserEntityAsync(User user)
        {
            var currentUserId = GetUserIdFromToken();
            if (user.Id != currentUserId)
                throw new UnauthorizedAccessException("No se puede modificar otro usuario");

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
