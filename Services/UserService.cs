using APISeasonalTicket.Data;
using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APISeasonalTicket.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public UserService(ApplicationDbContext context, IMapper mapper, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        // No mapeo a DTO en GetAll
        public async Task<List<User>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Obtener los roles del usuario usando UserManager
            var roles = await _userManager.GetRolesAsync(user);

            user.Roles = roles.ToList();

            return user;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }

        public async Task<User> GetUserByDNIAsync(string dni)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.DNI == dni);
            return user;
        }
        public async Task<bool> GetMailAvalibility(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return true;
            return false;
        }

        // Mapeo a UserDto en CreateUserAsync para devolverlo al cliente
        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.NormalizedEmail = user.Email.ToUpper();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        // Mapeo a UserDto en UpdateUserAsync
        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            var existingUser = await _context.Users.FindAsync(userDto.Id);

            if (existingUser == null)
            {
                throw new KeyNotFoundException($"No se encontró un usuario con ID {userDto.Id}");
            }

            _mapper.Map(userDto, existingUser);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(existingUser);
        }
        public async Task<User> UpdateUserAsyncDirect(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Mapeo a UserDto en DeleteUserAsync para devolverlo al cliente
        public async Task<UserDto> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }
    }
}
