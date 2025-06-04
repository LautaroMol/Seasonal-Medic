using APISeasonalTicket.Data;
using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveUserSubscriptionAsync(int userId, string subscriptionId)
        {
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionId = subscriptionId,
                Status = true,
                StartDate = DateTime.UtcNow
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
