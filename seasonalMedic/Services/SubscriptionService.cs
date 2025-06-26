using APISeasonalMedic.Data;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services.Interface;

namespace APISeasonalMedic.Services
{
    public class SubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveUserSubscriptionAsync(Guid userId, string subscriptionId)
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
