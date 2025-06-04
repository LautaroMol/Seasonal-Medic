using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;
using APISeasonalMedic.Services;
using System.Net.Http.Headers;
using MercadoPago.Config;
using System.Text;
using MercadoPago.Resource.Customer;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MercadoPago.Client.Payment;
using APISeasonalMedic.Services.Interface;
using System.Security.Claims;


namespace APISeasonalMedic.Controllers
{
    public class MercadoPagoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ICreditCardService _creditCardService;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MercadoPagoController> _logger;
        private readonly IMapper _mapper;

        public MercadoPagoController(IConfiguration configuration, IUserService userService, MercadoPagoService mercadoPagoService,
            ICreditCardService creditCardService,ApplicationDbContext context, IHttpClientFactory httpClientFactory,
            ILogger<MercadoPagoController> logger,IMapper mapper)
        {
            _configuration = configuration;
            _userService = userService;
            _mercadoPagoService = mercadoPagoService;
            _creditCardService = creditCardService;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View("~/Views/Home/Index.cshtml");
        }
        public IActionResult Error()
        {
            return View();
        }

        [HttpPost("guardar-tarjeta")]
        public async Task<IActionResult> GuardarTarjeta([FromBody] NewCreditCardDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos inválidos" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var guid))
                return Unauthorized();

            var usuario = await _userService.GetUserEntityByIdAsync(guid);

            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (string.IsNullOrEmpty(usuario.CustomerId))
                return BadRequest(new { message = "El usuario no tiene un CustomerId" });

            const int testAmount = 10;
            var accessToken = GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
                return StatusCode(500, new { message = "Token de Mercado Pago no configurado" });

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                // 1. Generar token de tarjeta
                var cardTokenRequest = new
                {
                    card_number = dto.CardNumber.ToString(),
                    expiration_month = dto.ExpirationMonth,
                    expiration_year = dto.ExpirationYear,
                    security_code = dto.SecurityCode,
                    cardholder = new
                    {
                        name = dto.CardholderName,
                        identification = new { type = dto.IdentificationType, number = dto.IdentificationNumber }
                    }
                };

                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/v1/card_tokens");
                tokenRequest.Content = JsonContent.Create(cardTokenRequest);
                tokenRequest.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

                var tokenResp = await httpClient.SendAsync(tokenRequest);
                var tokenJson = await tokenResp.Content.ReadAsStringAsync();

                if (!tokenResp.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error al generar token", detail = tokenJson });

                var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenJson);
                string cardToken = tokenData.id;

                // 2. Cobro de prueba CON captura inmediata
                var paymentRequest = new
                {
                    transaction_amount = testAmount,
                    token = cardToken,
                    description = "Verificación de tarjeta",
                    installments = 1,
                    payment_method_id = dto.CardType,
                    capture = true,
                    payer = new
                    {
                        type = "customer",
                        email = usuario.Email,
                        identification = new { type = dto.IdentificationType, number = dto.IdentificationNumber }
                    }
                };

                var payReq = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/v1/payments");
                payReq.Content = JsonContent.Create(paymentRequest);
                payReq.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

                var payResp = await httpClient.SendAsync(payReq);
                var payJson = await payResp.Content.ReadAsStringAsync();

                if (!payResp.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error en el cobro de prueba", detail = payJson });

                var payData = JsonConvert.DeserializeObject<dynamic>(payJson);
                if ((string)payData.status != "approved")
                    return BadRequest(new { message = "La tarjeta fue rechazada", status = (string)payData.status });

                long paymentId = payData.id;

                // 3. Reembolsar el pago inmediatamente utilizando el endpoint correcto
                var refundRequest = new { };  // Body vacío para reembolso total
                var refundReq = new HttpRequestMessage(HttpMethod.Post, $"https://api.mercadopago.com/v1/payments/{paymentId}/refunds");
                refundReq.Content = JsonContent.Create(refundRequest);
                refundReq.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

                var refundResp = await httpClient.SendAsync(refundReq);
                var refundJson = await refundResp.Content.ReadAsStringAsync();

                if (!refundResp.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error al reembolsar el pago", detail = refundJson });

                // 4. Guardar la tarjeta en Mercado Pago
                var saveCardRequest = new
                {
                    token = cardToken
                };

                var saveReq = new HttpRequestMessage(HttpMethod.Post, $"https://api.mercadopago.com/v1/customers/{usuario.CustomerId}/cards");
                saveReq.Content = JsonContent.Create(saveCardRequest);
                saveReq.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

                var saveResp = await httpClient.SendAsync(saveReq);
                var saveJson = await saveResp.Content.ReadAsStringAsync();

                if (!saveResp.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error al guardar la tarjeta en Mercado Pago", detail = saveJson });

                var cardData = JsonConvert.DeserializeObject<dynamic>(saveJson);
                string savedCardId = cardData.id;

                // 5. Guardar tarjeta en tu base de datos
                var tarjeta = new CreditCard
                {
                    CardId = savedCardId,
                    Last4Digits = dto.CardNumber.ToString().Substring(dto.CardNumber.ToString().Length - 4),
                    CardType = dto.CardType,
                    ExpirationMonth = dto.ExpirationMonth,
                    ExpirationYear = dto.ExpirationYear,
                    Token = cardToken,
                    UserId = usuario.Id,
                    CustomerId = usuario.CustomerId
                };

                _context.CreditCards.Add(tarjeta);
                usuario.CardToken = cardToken;
                await _userService.UpdateUserAsyncDirect(usuario);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tarjeta guardada correctamente", cardId = savedCardId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, detail = ex.StackTrace });
            }
        }


        [HttpPost("crear-customer")]
        public async Task<string> CreateOrGetCustomer(CustomerCreateDto dto)
        {
            var token = GetAccessToken();

            // 1? Buscar si ya existe el customer en Mercado Pago
            var user = await _userService.GetUserByEmailAsync(dto.Email);
            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(user.CustomerId))
            {
                return user.CustomerId; // Si ya tiene un CustomerId, lo devolvemos
            }

            // 2? Crear el cliente en Mercado Pago
            var client = new RestClient("https://api.mercadopago.com/v1/customers");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                email = dto.Email,
                first_name = dto.FirstName,
                last_name = dto.LastName
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error al crear customer: {response.Content}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            user.CustomerId = jsonResponse.id;
            await _userService.UpdateUserAsyncDirect(user);

            return user.CustomerId;
        }

        [HttpPost("probar-tarjeta")]
        public async Task<IActionResult> ProbarTarjeta([FromBody] NewCreditCardDto dto)
        {
            try
            {
                // Validar entrada
                if (dto == null || dto.UserId == Guid.Empty)
                    return BadRequest(new { message = "Datos inválidos" });

                // Buscar usuario y CustomerId
                var user = await _userService.GetUserEntityByIdAsync(dto.UserId);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                if (string.IsNullOrEmpty(user.CustomerId))
                    return BadRequest(new { message = "El usuario no tiene un CustomerId registrado" });

                // Configurar el AccessToken de MercadoPago
                MercadoPagoConfig.AccessToken = GetAccessToken();

                // Generar token de tarjeta
                var cardTokenRequest = new
                {
                    card_number = dto.CardNumber,
                    expiration_month = dto.ExpirationMonth,
                    expiration_year = dto.ExpirationYear,
                    security_code = dto.SecurityCode,
                    cardholder = new
                    {
                        name = dto.CardholderName,
                        identification = new
                        {
                            type = dto.IdentificationType,
                            number = dto.IdentificationNumber
                        }
                    }
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MercadoPagoConfig.AccessToken);

                var tokenResponse = await httpClient.PostAsJsonAsync("https://api.mercadopago.com/v1/card_tokens", cardTokenRequest);
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error al generar token de tarjeta", error = tokenJson });

                var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenJson);
                string cardToken = tokenData.id;

                // Realizar un pago de prueba
                var paymentRequest = new
                {
                    transaction_amount = 100, // Monto de prueba
                    token = cardToken, // Token de la tarjeta
                    description = "Pago de prueba con tarjeta",
                    installments = 1, // Número de cuotas
                    payment_method_id = dto.CardType, // Tipo de tarjeta (ej. "visa", "master")
                    payer = new
                    {
                        email = user.Email,
                        identification = new
                        {
                            type = dto.IdentificationType,
                            number = dto.IdentificationNumber
                        }
                    }
                };

                // Generar un valor único para el encabezado X-Idempotency-Key
                var idempotencyKey = Guid.NewGuid().ToString();
                httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

                var paymentResponse = await httpClient.PostAsJsonAsync("https://api.mercadopago.com/v1/payments", paymentRequest);
                var paymentJson = await paymentResponse.Content.ReadAsStringAsync();

                if (!paymentResponse.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Error en el cobro de prueba", error = paymentJson });

                var paymentData = JsonConvert.DeserializeObject<dynamic>(paymentJson);
                string paymentStatus = paymentData.status;

                // Verificar el estado del pago
                if (paymentStatus == "approved")
                {
                    return Ok(new { message = "Pago aprobado", paymentId = paymentData.id });
                }
                else
                {
                    return BadRequest(new { message = "Pago rechazado", status = paymentStatus });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetCustomerId")]
        public async Task<string?> GetCustomerIdByEmail(string email)
        {
            var token = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/v1/customers/search");
            var request = new RestRequest("", Method.Get);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddQueryParameter("email", email);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error al buscar customer: {response.Content}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            var results = jsonResponse.results;

            if (results.Count > 0)
            {
                return results[0].id;
            }

            return null; // Si no existe, retorna null
        }


        [HttpPost("crear-suscripcion")]
        public async Task<IActionResult> CrearSuscripcion([FromBody] SubscriptionDto dto)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(dto.Email);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                // Obtener la tarjeta activa del usuario
                var cards = await _creditCardService.GetCreditCardsByUserIdAsync(user.Id);
                var activeCard = cards.FirstOrDefault(c => c.IsActive); // Busca la tarjeta marcada como activa

                if (activeCard == null)
                    return BadRequest(new { message = "El usuario no tiene una tarjeta activa registrada" });

                // Generar nuevo token de tarjeta
                string newCardToken = await GenerateCardToken(activeCard.CustomerId, activeCard.CardId);

                var token = GetAccessToken();
                var client = new RestClient("https://api.mercadopago.com/preapproval");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    reason = "Pago mensual del abono",
                    external_reference = $"USER-{user.Id}",
                    payer_email = user.Email,
                    card_token_id = newCardToken, // Usa el nuevo token generado
                    auto_recurring = new
                    {
                        frequency = 1,
                        frequency_type = "months",
                        start_date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        end_date = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        transaction_amount = dto.Amount, // Usa el monto elegido por el usuario
                        currency_id = "ARS"
                    },
                    back_url = "https://tuapp.com/pago-exitoso",
                    status = "authorized"
                };

                request.AddJsonBody(body);
                var response = await client.ExecuteAsync(request);

                Console.WriteLine($"?? Respuesta de Mercado Pago: {response.Content}");

                if (!response.IsSuccessful)
                {
                    return BadRequest(new { message = "Error al crear suscripción", error = response.Content });
                }

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string subscriptionId = jsonResponse.id;

                // Guardar la suscripción en la base de datos
                var userSubscription = new UserSubscription
                {
                    UserId = user.Id,
                    SubscriptionId = subscriptionId,
                    Status = true,
                    StartDate = DateTime.UtcNow,
                    Amount = dto.Amount, //
                    CreditCardId = activeCard.Id // 
                };

                using var scope = HttpContext.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.UserSubscriptions.Add(userSubscription);
                await db.SaveChangesAsync();

                return Ok(new { message = "Suscripción creada exitosamente", subscriptionId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en el proceso", error = ex.Message });
            }
        }

        [HttpPost("marcar-tarjeta-activa/{creditCardId}")]
        public async Task<IActionResult> MarcarTarjetaActiva(Guid creditCardId, Guid userId)
        {
            var cards = await _creditCardService.GetCreditCardsByUserIdAsync(userId);

            if (!cards.Any())
                return NotFound(new { message = "No se encontraron tarjetas para este usuario" });

            foreach (var card in cards)
            {
                card.IsActive = card.Id == creditCardId; // Solo la seleccionada se marca como activa
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tarjeta principal actualizada" });
        }
        [HttpGet("Cards/customerId")]
        public async Task<IActionResult> CardsByClientId(string customerId)
        {
            var token = GetAccessToken();
            var client = new RestClient($"https://api.mercadopago.com/v1/customers/{customerId}/cards");
            var request = new RestRequest("", Method.Get);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                return StatusCode(500, new { message = "Error al obtener tarjetas", error = response.Content });
            }
            return Ok(response.Content);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> MakePayment([FromBody] MakePaymentDto dto)
        {
            try
            {
                // Configurar el AccessToken de MercadoPago
                MercadoPagoConfig.AccessToken = GetAccessToken();

                // Obtener el usuario
                var user = await _userService.GetUserEntityByIdAsync(dto.UserId)
                    ?? throw new Exception("Usuario no encontrado");

                // Obtener la tarjeta
                var card = await _creditCardService.GetCreditCardByIdAsync(dto.CardId)
                    ?? throw new Exception("Tarjeta no encontrada");

                if (card.UserId != dto.UserId)
                    throw new Exception("La tarjeta no pertenece al usuario");

                if (string.IsNullOrEmpty(card.CardType))
                    throw new Exception("El tipo de tarjeta (CardType) no puede estar vacío");

                // Generar el token de la tarjeta
                var token = await GenerateCardTokenWithSDK(dto.UserId, dto.CardId, dto.SecurityCode);

                // Crear el objeto de pago
                var paymentRequest = new
                {
                    transaction_amount = dto.Amount, // Monto del pago
                    token = token, // Token de la tarjeta
                    description = "Pago de producto", // Descripción del producto
                    installments = 1, // Número de cuotas
                    payment_method_id = card.CardType, // Tipo de tarjeta (ej. "visa", "master")
                    payer = new
                    {
                        email = user.Email, // Email del pagador
                        identification = new
                        {
                            type = "DNI", // Tipo de identificación
                            number = user.DNI // Número de identificación
                        }
                    }
                };

                // Configurar las opciones de la solicitud
                var idempotencyKey = Guid.NewGuid().ToString(); // Generar un valor único para el encabezado
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MercadoPagoConfig.AccessToken);
                httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey); // Agregar el encabezado

                // Enviar la solicitud al endpoint de MercadoPago
                var response = await httpClient.PostAsJsonAsync("https://api.mercadopago.com/v1/payments", paymentRequest);

                // Leer la respuesta
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = "Error al procesar el pago", error = responseContent });
                }

                // Parsear la respuesta
                var paymentResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // Verificar el estado del pago
                if (paymentResponse.status == "approved")
                {
                    return Ok(new { message = "Pago aprobado", paymentId = paymentResponse.id });
                }
                else
                {
                    return BadRequest(new { message = "Pago rechazado", status = paymentResponse.status });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("api/Deletecard/{cardId}")]
        public async Task<object> DeleteCardCustomerAsync(int cardId)
        {
            var creditCard = await _context.CreditCards.FindAsync(cardId);

            if (creditCard == null)
                throw new InvalidOperationException("La tarjeta no existe.");

            if (creditCard.IsPrimary)
                throw new InvalidOperationException("No se puede eliminar la tarjeta principal.");

            var totalCards = await _context.CreditCards
                .CountAsync(c => c.UserId == creditCard.UserId);

            if (totalCards == 1)
                throw new InvalidOperationException("No se puede eliminar la única tarjeta registrada.");

            var accessToken = GetAccessToken();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"https://api.mercadopago.com/v1/customers/{creditCard.CustomerId}/cards/{creditCard.CardId}";

            var response = await httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error al eliminar la tarjeta en MercadoPago: {error}");
            }

            _context.CreditCards.Remove(creditCard);
            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = $"La tarjeta terminada en {creditCard.Last4Digits} fue eliminada correctamente.",
                cardId = creditCard.Id
            };
        }

        private async Task<string> GenerateCardTokenWithSDK(Guid userId, int cardDbId, string securityCode)
        {
            // Configurar el access token antes de usar el SDK
            MercadoPagoConfig.AccessToken = GetAccessToken();

            var user = await _userService.GetUserEntityByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.CustomerId))
                throw new Exception("Usuario inválido o sin CustomerId");

            var card = await _creditCardService.GetCreditCardByIdAsync(cardDbId);
            if (card == null || card.UserId != userId)
                throw new Exception("Tarjeta no encontrada o no pertenece al usuario");

            if (string.IsNullOrEmpty(card.CardId))
                throw new Exception("CardId (de MercadoPago) no está guardado en la base de datos");

            var client = new MercadoPago.Client.CardToken.CardTokenClient();

            var cardTokenRequest = new MercadoPago.Client.CardToken.CardTokenRequest
            {
                CardId = card.CardId,
                CustomerId = user.CustomerId,
                SecurityCode = securityCode
            };

            var cardToken = await client.CreateAsync(cardTokenRequest);

            if (string.IsNullOrEmpty(cardToken?.Id))
                throw new Exception("? No se pudo generar el token de la tarjeta");

            Console.WriteLine($"? Token generado correctamente: {cardToken.Id}");
            return cardToken.Id;
        }

        private async Task<string> GenerateCardToken(string customerId, string cardId)
        {
            var token = GetAccessToken();
            var client = new RestClient($"https://api.mercadopago.com/v1/card_tokens");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                card_id = cardId,
                customer_id = customerId
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);

            Console.WriteLine($"?? Respuesta de generación de token: {response.Content}");

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error al generar token de tarjeta: {response.Content}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return jsonResponse.id;
        }


        private async Task<string> CreateSubscriptionPlan()
        {
            var token = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/preapproval_plan");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                back_url = "https://tuapp.com",
                reason = "Plan Mensual de Abono",
                auto_recurring = new
                {
                    frequency = 1,
                    frequency_type = "months",
                    transaction_amount = 100.00,  // Monto de la suscripción
                    currency_id = "ARS",
                    start_date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    end_date = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"?? Respuesta de creación del plan: {response.Content}");

            if (!response.IsSuccessful)
            {
                throw new Exception($"? Error al crear el plan de suscripción: {response.Content}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return jsonResponse.id;
        }

        private string GetAccessToken()
        {
            var token = _configuration.GetValue<string>("MercadoPago:AccessToken");
            return token ?? string.Empty;
        }
        private string GetPublicKey()
        {
            var token = _configuration.GetValue<string>("MercadoPago:PublicKey");
            return token ?? string.Empty;
        }
    }
}
