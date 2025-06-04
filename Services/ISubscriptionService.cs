
namespace APISeasonalTicket.Services
{
    public interface ISubscriptionService
    {
        Task<bool> SaveUserSubscriptionAsync(int userId, string subscriptionId);
    }
}