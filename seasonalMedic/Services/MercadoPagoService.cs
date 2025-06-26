using APISeasonalMedic.Data;
using APISeasonalMedic.Models;
using Azure.Core;
using MercadoPago.Client.Customer;
using MercadoPago.Config;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http.Headers;

namespace APISeasonalMedic.Services
{
    public class MercadoPagoService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public MercadoPagoService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SaveCreditCard(User user, string cardToken)
        {
            var customerClient = new CustomerClient();

            // Crear cliente en Mercado Pago si no existe
            if (string.IsNullOrEmpty(user.CustomerId))
            {
                var customerRequest = new CustomerRequest { Email = user.Email };
                var customer = await customerClient.CreateAsync(customerRequest);
                user.CustomerId = customer.Id;
            }

            // Guardar tarjeta en Mercado Pago
            var cardRequest = new CustomerCardCreateRequest { Token = cardToken };
            var card = await customerClient.CreateCardAsync(user.CustomerId, cardRequest);

            // Guardar tarjeta en base de datos
            var creditCard = new CreditCard
            {
                Last4Digits = card.LastFourDigits,
                Token = cardToken,
                CardType = card.PaymentMethod.Id,
                ExpirationMonth = (int)card.ExpirationMonth,
                ExpirationYear = (int)card.ExpirationYear,
                CardId = card.Id,
                CustomerId = user.CustomerId,
                UserId = user.Id
            };

            _context.CreditCards.Add(creditCard);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetCustomerIdByEmail(string email)
        {
            var _accessToken = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/v1/customers/search");
            var request = new RestRequest("", Method.Get);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            request.AddQueryParameter("email", email);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return null; // Si hay error, devolvemos null
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            var results = jsonResponse.results;

            return results.Count > 0 ? results[0].id : null;
        }

        public async Task<string?> CreateMercadoPagoCustomer(string email, string firstName, string lastName)
        {
            var _accessToken = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/v1/customers");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            request.AddHeader("Content-Type", "application/json");

            var body = new { email, first_name = firstName, last_name = lastName };
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return null;

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return jsonResponse.id;
        }
        public async Task<string?> CreateSubscriptionAsync(User user, decimal monto)
        {
            var _accessToken = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/preapproval");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            request.AddJsonBody(new
            {
                payer_email = user.Email,
                back_url = "https://tusitio.com/suscripcion",
                reason = "Suscripción mensual",
                auto_recurring = new
                {
                    frequency = 1,
                    frequency_type = "months",
                    transaction_amount = monto,
                    currency_id = "ARS",
                    start_date = DateTime.UtcNow,
                    end_date = DateTime.UtcNow.AddYears(1)
                }
            });

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return null;

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return jsonResponse.id;
        }

        public async Task<bool> SaveCardAsync(string customerId, string cardToken)
        {
            var _accessToken = GetAccessToken();
            var client = new RestClient("https://api.mercadopago.com/v1/card");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            request.AddJsonBody(new { customer_id = customerId, token = cardToken });

            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful;
        }

        private string GetAccessToken()
        {
            var token = _configuration.GetValue<string>("MercadoPago:AccessToken");
            return token ?? string.Empty;
        }

        public async Task<bool> SaveUserSubscriptionAsync(Guid userId, string subscriptionId)
        {
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionId = subscriptionId
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
