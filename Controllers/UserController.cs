using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;
using APISeasonalTicket.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APISeasonalTicket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private UserManager<User> _userManager;
        private IMessageService _mailService;
        private readonly IConfiguration _configuration;
        private readonly MercadoPagoService _mercadoPagoService;
        public UserController(IUserService userService, IMessageService mailService, UserManager<User> userManager,
            IConfiguration configuration, MercadoPagoService mercadoPagoService)
        {
            _userService = userService;
            _mailService = mailService;
            _userManager = userManager;
            _configuration = configuration;
            _mercadoPagoService = mercadoPagoService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAll();  // Obtiene la lista de User directamente
            return Ok(users);  // Devuelve la lista de User
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);  // Obtiene el User por ID
            if (user == null)
                return NotFound();  // Si no se encuentra, devuelve 404
            return Ok(user);  // Devuelve el User
        }


        //Get by Dni
        [HttpGet("/dni/{dni}")]
        public async Task<IActionResult> GetByDni(string dni)
        {
            var user = await _userService.GetUserByDNIAsync(dni);
            return Ok(user);
        }
        //Comprobar email
        [HttpGet("/email/{email}")]
        public async Task<bool> GetMailAvalibility(string email)
        {
            bool value = await _userService.GetMailAvalibility(email);
            return value;
        }
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
        //Post
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(UserDto userdto)
        {
            var created = await _userService.CreateUserAsync(userdto);
            return Ok(created);
        }

        [HttpPut]
        public async Task<IActionResult> Put(UserDto userdto)
        {
            Console.WriteLine($"Actualizando usuario con ID: {userdto.Id}");

            var updated = await _userService.UpdateUserAsync(userdto);

            return Ok(updated);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            return Ok(deleted);
        }

        //usuarios
        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            try
            {
                // Validación de DNI: Solo números y máximo 9 caracteres
                if (!Regex.IsMatch(register.DNI, @"^\d{1,9}$"))
                {
                    return BadRequest(new { error = "El DNI debe ser un número y tener máximo 9 caracteres." });
                }

                // Validación de Email
                if (!Regex.IsMatch(register.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    return BadRequest(new { error = "El email no es válido." });
                }

                // Validación de Apellidos: Solo letras y espacios
                if (!Regex.IsMatch(register.LastName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                {
                    return BadRequest(new { error = "Los apellidos solo deben contener letras y espacios." });
                }
                if (string.IsNullOrWhiteSpace(register.Email))
                {
                    return BadRequest(new { error = "El correo electrónico no es válido." });
                }

                var customerId = await _mercadoPagoService.GetCustomerIdByEmail(register.Email);
                if (string.IsNullOrEmpty(customerId))
                {
                    customerId = await _mercadoPagoService.CreateMercadoPagoCustomer(register.Email, register.FirstName, register.LastName);
                    if (string.IsNullOrEmpty(customerId))
                    {
                        return StatusCode(500, new { error = "Error al registrar el usuario en Mercado Pago." });
                    }
                }

                string? imageUrl = null;

                if (register.ProfileImage != null && register.ProfileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(register.ProfileImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { error = "Solo se permiten imágenes con las extensiones .jpg, .jpeg, .png, .gif" });
                    }

                    var folderPath = Path.Combine("wwwroot", "images", "profiles");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(register.ProfileImage.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await register.ProfileImage.CopyToAsync(stream);
                    }

                    imageUrl = $"{Request.Scheme}://{Request.Host}/images/profiles/{fileName}";
                }

                var user = new User
                {
                    UserName = register.Email,
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    AreaCode = register.AreaCode,
                    Email = register.Email,
                    NormalizedEmail = register.Email.ToUpper(),
                    DNI = register.DNI,
                    PhoneNumber = register.PhoneNumber,
                    EmailConfirmed = false,
                    CustomerId = customerId,
                    ProfileImageUrl = imageUrl 
                };

                var result = await _userManager.CreateAsync(user, register.Password);
                if (!result.Succeeded)
                    return BadRequest(new { error = result.Errors });

                var role = register.Email == "laumol159@gmail.com" ? "Admin" : "User";
                await _userManager.AddToRoleAsync(user, role);

                var verificationCode = new Random().Next(100000, 999999).ToString();
                user.VerificationCode = verificationCode;
                await _userManager.UpdateAsync(user);

                var body = $"<p>Tu código de verificación es: <strong>{verificationCode}</strong></p>";
                _mailService.SendEmail(register.Email, "Código de verificación", body);

                return Ok(new { message = "Usuario creado. Revisá tu email para obtener el código de confirmación." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado.", details = ex.Message });
            }
        }


        public class ResendVerificationRequest
        {
            public string Email { get; set; }
        }
        [HttpPost("ResendVerificationCode")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("El email es requerido.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("El correo ya está confirmado.");
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = verificationCode;
            await _userManager.UpdateAsync(user);

            var body = $"<p>Tu código de verificación es: <strong>{verificationCode}</strong></p>";
            _mailService.SendEmail(user.Email, "Código de verificación", body);

            return Ok(new { message = "Se ha enviado un nuevo código de verificación al correo." });

        }
        public class ConfirmEmailDto
        {
            public string Email { get; set; }
            public string VerificationCode { get; set; }
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.VerificationCode))
                return BadRequest("Email y código de verificación son requeridos.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("Usuario no encontrado");

            if (user.VerificationCode == model.VerificationCode)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return Ok(new { success = true, message = "Email confirmado correctamente." });
            }

            return BadRequest(new { success = false, message = "Código incorrecto." });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Usuario no encontrado.");

            var isValidPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isValidPassword)
                return Unauthorized("Contraseña incorrecta.");

            // Obtener roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generar claims
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Devuelve el ID del usuario
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMonths(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                userId = user.Id, 
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }


        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserDto>> ObtenerUsuarioActual()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var dto = new UserDto
            {
                Id = usuario.Id,
                DNI = usuario.DNI,
                FirstName = usuario.FirstName,
                LastName = usuario.LastName,
                Email = usuario.Email,
                AreaCode = usuario.AreaCode ?? "",
                PhoneNumber = usuario.PhoneNumber,
                CustomerId = usuario.CustomerId,
                ProfileImageUrl = usuario.ProfileImageUrl
            };

            return Ok(dto);
        }


        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserinfo()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized();
                }

                var user = await _userService.GetUserByIdAsync(int.Parse(userId));

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user); 
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción interna
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task<string> CreateMercadoPagoCustomer(string email, string firstName, string lastName)
        {
            var token = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/v1/customers");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                email,
                first_name = firstName,
                last_name = lastName
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Error al crear customer: {response.Content}");
                return null;
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return jsonResponse.id;
        }

        private string GetAccessToken()
        {
            var token = _configuration.GetValue<string>("MercadoPago:AccessToken");
            return token ?? string.Empty;
        }

    }
}
