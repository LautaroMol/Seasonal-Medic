using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services;
using APISeasonalMedic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static APISeasonalMedic.Services.UserService;


namespace APISeasonalMedic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private IMessageService _mailService;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly IConfiguration _configuration;
        private UserManager<User> _userManager;
        private readonly CloudinaryService _cloudinaryService;
        public UserController(IUserService userService, MercadoPagoService mercadoPagoService,IMessageService mailService,
            IConfiguration configuration,UserManager<User> userManager,CloudinaryService cloudinaryService)
        {
            _userService = userService;
            _mercadoPagoService = mercadoPagoService;
            _mailService = mailService;
            _configuration = configuration;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cloudinaryService = cloudinaryService;

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            try
            {
                var userDto = await _userService.RegisterAsync(register);
                return Ok(new
                {
                    message = "Usuario creado exitosamente. Revisa tu email para obtener el código de confirmación.",
                    user = userDto
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado durante el registro.", details = ex.Message });
            }
        }

        [HttpPost("resend-code")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendCode([FromQuery] string email)
        {
            try
            {
                await _userService.ResendVerificationCodeAsync(email);
                return Ok(new { message = "Código reenviado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "EMAIL_ALREADY_CONFIRMED", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "SERVER_ERROR", message = ex.Message });
            }
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                return BadRequest(new { success = false, message = "Email y código de verificación son requeridos." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Usuario no encontrado." });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new { success = true, message = "El email ya fue confirmado previamente." });
            }

            if (user.VerificationCode != model.VerificationCode)
            {
                return BadRequest(new { success = false, message = "Código de verificación incorrecto." });
            }

             // validacion de que no este expirado
             if (user.VerificationCodeExpiry.HasValue && user.VerificationCodeExpiry < DateTime.UtcNow)
             {
                 return BadRequest(new { success = false, message = "El código de verificación ha expirado." });
            }

            user.EmailConfirmed = true;
            user.VerificationCode = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(500, new { success = false, message = "Error al actualizar el estado de confirmación." });
            }

            return Ok(new { success = true, message = "Email confirmado correctamente." });
        }
        // POST: api/user/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var result = await _userService.LoginAsync(dto);
                return Ok(result);
            }
            catch (EmailNotConfirmedException ex)
            {
                return StatusCode(403, new
                {
                    error = "EMAIL_NOT_CONFIRMED",
                    message = ex.Message,
                    info = ex.Info
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "INVALID_CREDENTIALS", message = ex.Message });
            }
        }

        // GET: api/user/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                // Usa el método específico del service que ya maneja la extracción del token
                var result = await _userService.GetCurrentUserDtoAsync();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/user/me
        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UserDto dto)
        {
            try
            {
                // El service ya maneja la validación del token internamente
                var updatedUser = await _userService.UpdateUserDtoAsync(dto);
                return Ok(updatedUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/user/me
        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            try
            {
                // Obtén el usuario actual para conseguir su ID
                var currentUser = await _userService.GetCurrentUserEntityAsync();
                var deleted = await _userService.DeleteUserAsync(currentUser.Id);
                return deleted ? NoContent() : NotFound("No se pudo eliminar el usuario");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/user/check-email?email=algo@ejemplo.com
        [HttpGet("check-email")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CheckEmailAvailability([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email es requerido");

            var available = await _userService.GetMailAvailabilityAsync(email);
            return Ok(available);
        }
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
        //imagenes
        [Authorize]
        [HttpPut("profile-image")]
        public async Task<IActionResult> UpdateProfileImageUrl([FromBody] UpdateProfileImageUrlDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("Usuario no encontrado");

                // OBTENER LA URL ANTERIOR ANTES DE ACTUALIZAR
                var oldImageUrl = user.ProfileImageUrl;

                // Actualizar la nueva imagen en la base de datos
                user.ProfileImageUrl = dto.ProfileImageUrl;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest("No se pudo actualizar la imagen de perfil.");

                // Eliminar la imagen anterior de Cloudinary (si existe y es diferente)
                if (!string.IsNullOrEmpty(oldImageUrl) &&
                    oldImageUrl != dto.ProfileImageUrl &&
                    oldImageUrl.Contains("cloudinary.com"))
                {
                    // ELIMINAR INMEDIATAMENTE, NO EN BACKGROUND
                    try
                    {
                        var deleteResult = await _cloudinaryService.DeleteImageAsync(oldImageUrl);
                        if (!deleteResult)
                        {
                            // Log warning but don't fail the request
                            Console.WriteLine($"Warning: No se pudo eliminar la imagen anterior: {oldImageUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the request
                        Console.WriteLine($"Error eliminando imagen anterior: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = "Imagen actualizada correctamente",
                    imageUrl = dto.ProfileImageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete("profile-image")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("Usuario no encontrado");

                var currentImageUrl = user.ProfileImageUrl;

                // Verificar si hay imagen para eliminar
                if (string.IsNullOrEmpty(currentImageUrl))
                {
                    return Ok(new { message = "No hay imagen para eliminar" });
                }

                // PRIMERO eliminar de Cloudinary ANTES de actualizar la base de datos
                if (currentImageUrl.Contains("cloudinary.com"))
                {
                    try
                    {
                        Console.WriteLine($"Intentando eliminar imagen de Cloudinary: {currentImageUrl}");
                        var deleteResult = await _cloudinaryService.DeleteImageAsync(currentImageUrl);

                        if (!deleteResult)
                        {
                            Console.WriteLine("Warning: No se pudo eliminar la imagen de Cloudinary");
                            // Continuar de todos modos para limpiar la base de datos
                        }
                        else
                        {
                            Console.WriteLine("Imagen eliminada exitosamente de Cloudinary");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error eliminando de Cloudinary: {ex.Message}");
                        // Continuar de todos modos para limpiar la base de datos
                    }
                }

                // DESPUÉS actualizar la base de datos
                user.ProfileImageUrl = string.Empty;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest("No se pudo eliminar la imagen de perfil de la base de datos.");

                return Ok(new { message = "Imagen eliminada correctamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general en DeleteProfileImage: {ex.Message}");
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    message = ex.Message
                });
            }
        }
    }
}