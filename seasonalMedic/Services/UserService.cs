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
using System.Text.RegularExpressions;

namespace APISeasonalMedic.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IMessageService _mailService;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public UserService(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            IMessageService mailService,
            MercadoPagoService mercadoPagoService,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _mailService = mailService;
            _mercadoPagoService = mercadoPagoService;
            _roleManager = roleManager;
        }

        private Guid GetUserIdFromToken()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId ?? throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario."));
        }


        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            // Validaciones básicas
            if (dto == null)
                throw new ArgumentException("Datos de registro no pueden estar vacíos.");

            if (string.IsNullOrWhiteSpace(dto.Dni) || !Regex.IsMatch(dto.Dni, @"^\d{1,9}$"))
                if (string.IsNullOrWhiteSpace(dto.Dni) || !Regex.IsMatch(dto.Dni, @"^\d{1,9}$"))
                    throw new ArgumentException("El DNI debe ser un número y tener máximo 9 caracteres.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !Regex.IsMatch(dto.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                throw new ArgumentException("El email no es válido.");

            if (string.IsNullOrWhiteSpace(dto.FirstName) || !Regex.IsMatch(dto.FirstName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                throw new ArgumentException("El nombre solo debe contener letras y espacios.");

            if (string.IsNullOrWhiteSpace(dto.LastName) || !Regex.IsMatch(dto.LastName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                throw new ArgumentException("Los apellidos solo deben contener letras y espacios.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !Regex.IsMatch(dto.PhoneNumber, @"^\d{8,15}$"))
                throw new ArgumentException("El número de teléfono debe contener solo números y tener entre 8 y 15 dígitos.");

            // Verificar si usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Ya existe un usuario registrado con este email.");

            // Obtener o crear CustomerId en MercadoPago
            string customerId;
            try
            {
                customerId = await _mercadoPagoService.GetCustomerIdByEmail(dto.Email);
                if (string.IsNullOrEmpty(customerId))
                {
                    customerId = await _mercadoPagoService.CreateMercadoPagoCustomer(dto.Email, dto.FirstName, dto.LastName);
                    if (string.IsNullOrEmpty(customerId))
                        throw new Exception("Error al registrar el usuario en Mercado Pago.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al procesar el registro con el servicio de pagos: " + ex.Message);
            }

            // Crear usuario
            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;
            user.Email = dto.Email;
            user.NormalizedEmail = dto.Email.ToUpper();
            user.CustomerId = customerId;
            user.EmailConfirmed = false;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException("Error al crear el usuario: " + errors);
            }

            // Asignar rol
            var role = dto.Email == "laumol159@gmail.com" ? "Admin" : "User";
            try
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            catch (Exception ex)
            {
                // Loggear el error pero no tirar excepción para no romper flujo
                Console.WriteLine($"Error al asignar rol: {ex.Message}");
            }

            // Generar código de verificación y expiración
            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(30);
            await _userManager.UpdateAsync(user);

            // Enviar email (excepción no detiene el flujo)
            try
            {
                var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>¡Bienvenido a Seasonal Medic!</h2>
                <p>Hola {dto.FirstName},</p>
                <p>Gracias por registrarte en nuestra plataforma. Para completar tu registro, por favor ingresa el siguiente código de verificación:</p>
                <div style='background-color: #f8f9fa; border: 2px solid #007bff; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
                    <h1 style='color: #007bff; font-size: 36px; margin: 0; letter-spacing: 5px;'>{verificationCode}</h1>
                </div>
                <p><strong>Este código es válido por 30 minutos.</strong></p>
                <p>Si no solicitaste este registro, puedes ignorar este email.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                <p style='color: #666; font-size: 12px;'>
                    Este es un email automático, por favor no respondas a este mensaje.<br>
                    © 2025 Seasonal Medic. Todos los derechos reservados.
                </p>
            </div>";

                _mailService.SendEmail(dto.Email, "Código de verificación - Seasonal Medic", emailBody);
            }
            catch (Exception ex)
            {
                // Log y continúa (el usuario ya fue creado)
                Console.WriteLine($"Error al enviar email: {ex.Message}");
            }

            return _mapper.Map<UserDto>(user);
        }
        public async Task<LoginResponseDto> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            if (!user.EmailConfirmed)
            {
                var codeStillValid = user.VerificationCodeExpiry.HasValue && user.VerificationCodeExpiry > DateTime.UtcNow;
                throw new EmailNotConfirmedException("Correo no confirmado", new EmailConfirmationInfo
                {
                    Email = user.Email,
                    CodeSent = !string.IsNullOrEmpty(user.VerificationCode),
                    CodeStillValid = codeStillValid,
                    Expiry = user.VerificationCodeExpiry
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            // Aquí está el cambio: pasar user.EmailConfirmed
            var result = _jwtService.GenerateToken(user.Id, user.Email, roles.ToList(), user.EmailConfirmed);

            return new LoginResponseDto
            {
                Token = result.Token,
                ExpiresAt = result.ExpiresAt
            };
        }
        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("El correo ya fue confirmado");

            // Generar nuevo código
            var newCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = newCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(30);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new Exception("Error al actualizar el código de verificación");

            // Armar email HTML
            var emailBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #333;'>Reenvío de Código - Seasonal Medic</h2>
            <p>Hola {user.FirstName},</p>
            <p>Has solicitado un nuevo código de verificación para completar tu registro.</p>
            <div style='background-color: #f8f9fa; border: 2px solid #007bff; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
                <h1 style='color: #007bff; font-size: 36px; margin: 0; letter-spacing: 5px;'>{newCode}</h1>
            </div>
            <p><strong>Este código es válido por 30 minutos.</strong></p>
            <p>Si no solicitaste este reenvío, puedes ignorar este email.</p>
            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
            <p style='color: #666; font-size: 12px;'>
                Este es un email automático, por favor no respondas a este mensaje.<br>
                © 2025 Seasonal Medic. Todos los derechos reservados.
            </p>
        </div>
    ";

            _mailService.SendEmail(
                user.Email,
                "Código de verificación - Seasonal Medic",
                emailBody
            );

            return true;
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
        public class EmailNotConfirmedException : Exception
        {
            public EmailConfirmationInfo Info { get; }

            public EmailNotConfirmedException(string message, EmailConfirmationInfo info)
                : base(message)
            {
                Info = info;
            }
        }

        #region Seccion de agentes
        // Método RegisterAgentAsync corregido con verificación de roles:

        public async Task<UserDto> RegisterAgentAsync(RegisterDto dto)
        {
            // Validaciones básicas (reutilizar las mismas del RegisterAsync original)
            if (dto == null)
                throw new ArgumentException("Datos de registro no pueden estar vacíos.");

            if (string.IsNullOrWhiteSpace(dto.Dni) || !Regex.IsMatch(dto.Dni, @"^\d{1,9}$"))
                throw new ArgumentException("El DNI debe ser un número y tener máximo 9 caracteres.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !Regex.IsMatch(dto.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                throw new ArgumentException("El email no es válido.");

            if (string.IsNullOrWhiteSpace(dto.FirstName) || !Regex.IsMatch(dto.FirstName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                throw new ArgumentException("El nombre solo debe contener letras y espacios.");

            if (string.IsNullOrWhiteSpace(dto.LastName) || !Regex.IsMatch(dto.LastName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                throw new ArgumentException("Los apellidos solo deben contener letras y espacios.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !Regex.IsMatch(dto.PhoneNumber, @"^\d{8,15}$"))
                throw new ArgumentException("El número de teléfono debe contener solo números y tener entre 8 y 15 dígitos.");

            // Verificar si usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Ya existe un usuario registrado con este email.");

            // VERIFICAR QUE EL ROL EXISTE
            const string agentRole = "Agente";
            if (!await _roleManager.RoleExistsAsync(agentRole))
            {
                throw new InvalidOperationException($"El rol '{agentRole}' no existe en el sistema. Contacte al administrador.");
            }

            // Crear usuario (sin CustomerId para agentes)
            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;
            user.Email = dto.Email;
            user.NormalizedEmail = dto.Email.ToUpper();
            user.CustomerId = null; // Los agentes no necesitan MercadoPago
            user.EmailConfirmed = false;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException("Error al crear el usuario: " + errors);
            }

            // Asignar rol de Agente automáticamente con manejo de errores mejorado
            try
            {
                var roleResult = await _userManager.AddToRoleAsync(user, agentRole);
                if (!roleResult.Succeeded)
                {
                    // Si falla la asignación de rol, eliminar el usuario creado
                    await _userManager.DeleteAsync(user);
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Error al asignar rol de agente: {roleErrors}");
                }
            }
            catch (Exception ex)
            {
                // Si falla la asignación de rol, eliminar el usuario creado para mantener consistencia
                try
                {
                    await _userManager.DeleteAsync(user);
                }
                catch
                {
                    // Ignore cleanup errors
                }
                throw new InvalidOperationException($"Error al asignar rol de agente al usuario: {ex.Message}");
            }

            // Generar código de verificación y expiración
            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(30);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Si falla la actualización, eliminar el usuario
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException("Error al actualizar el código de verificación del usuario.");
            }

            // Enviar email de verificación para agente
            try
            {
                var emailBody = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #333;'>¡Bienvenido como Agente a Seasonal Medic!</h2>
    <p>Hola {dto.FirstName},</p>
    <p>Tu cuenta de agente ha sido creada exitosamente. Para completar tu registro, por favor ingresa el siguiente código de verificación:</p>
    <div style='background-color: #f8f9fa; border: 2px solid #28a745; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
        <h1 style='color: #28a745; font-size: 36px; margin: 0; letter-spacing: 5px;'>{verificationCode}</h1>
    </div>
    <p><strong>Este código es válido por 30 minutos.</strong></p>
    <p>Como agente tendrás acceso a funcionalidades especiales para gestionar usuarios.</p>
    <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
    <p style='color: #666; font-size: 12px;'>
        Este es un email automático, por favor no respondas a este mensaje.<br>
        © 2025 Seasonal Medic. Todos los derechos reservados.
    </p>
</div>";

                _mailService.SendEmail(dto.Email, "Cuenta de Agente creada - Seasonal Medic", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar email: {ex.Message}");
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            // Solo obtener usuarios con rol "User"
            var usersInRole = await _userManager.GetUsersInRoleAsync("User");
            return _mapper.Map<List<UserDto>>(usersInRole.ToList());
        }

        public async Task<UserDto> GetUserByDniAsync(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || !Regex.IsMatch(dni, @"^\d{1,9}$"))
                throw new ArgumentException("El DNI debe ser un número y tener máximo 9 caracteres.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DNI == dni);
            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado con el DNI proporcionado.");

            // Verificar que el usuario tenga rol "User"
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains("User"))
                throw new UnauthorizedAccessException("Solo se pueden buscar usuarios con rol 'User'.");

            return _mapper.Map<UserDto>(user);
        }

        // Método auxiliar para verificar si el usuario actual es Agente
        public async Task<bool> IsCurrentUserAgentAsync()
        {
            try
            {
                var currentUser = await GetCurrentUserEntityAsync();
                var roles = await _userManager.GetRolesAsync(currentUser);
                return roles.Contains("Agente") || roles.Contains("Admin");
            }
            catch
            {
                return false;
            }
        }

        // Método para obtener el rol del usuario actual
        public async Task<List<string>> GetCurrentUserRolesAsync()
        {
            var currentUser = await GetCurrentUserEntityAsync();
            var roles = await _userManager.GetRolesAsync(currentUser);
            return roles.ToList();
        }

        #endregion
    }
}
