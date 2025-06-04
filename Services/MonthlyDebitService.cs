using APISeasonalTicket.Data;
using APISeasonalTicket.DTOs;
using APISeasonalTicket.Migrations;
using APISeasonalTicket.Models;
using Azure.Core;
using MercadoPago.Config;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

public class MonthlyDebitService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MonthlyDebitService> _logger;

    public MonthlyDebitService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<MonthlyDebitService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 MonthlyDebitService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarDebitos(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("🚨 ProcesarDebitos fue cancelado.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en MonthlyDebitService: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }


    public async Task ProcesarDebitos(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var suscripciones = await db.UserSubscriptions
                .Where(s => s.Status)
                .ToListAsync(stoppingToken);

            foreach (var suscripcion in suscripciones)
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

                var response = await httpClient.GetAsync(
                    $"https://api.mercadopago.com/preapproval/{suscripcion.SubscriptionId}",
                    stoppingToken);

                if (!response.IsSuccessStatusCode) continue;

                var subscriptionData = await response.Content.ReadFromJsonAsync<SubscriptionResponse>(cancellationToken: stoppingToken);
                if (subscriptionData != null && subscriptionData.status == "authorized")
                {
                    _logger.LogInformation($"✅ Pago automático realizado para usuario {suscripcion.UserId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error en ProcesarDebitos: {ex.Message}");
        }
    }




    private string GetAccessToken()
    {
        var token = _configuration.GetValue<string>("MercadoPago:AccessToken");
        return token ?? string.Empty;
    }

}


