using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    /// <summary>
    /// Background service to automatically delete expired guest users.
    /// Runs every minute to check for users that have exceeded their ExpiresAt time.
    /// </summary>
    public class GuestUserCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GuestUserCleanupService> _logger;

        public GuestUserCleanupService(IServiceProvider serviceProvider, ILogger<GuestUserCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GuestUserCleanupService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredGuestsAsync();
                    // Run every minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GuestUserCleanupService");
                }
            }

            _logger.LogInformation("GuestUserCleanupService stopped");
        }

        private async Task CleanupExpiredGuestsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RepositoryContext>();

                // Find all guest users where ExpiresAt <= now
                var expiredGuests = context.Users
                    .Where(u => u.IsGuest && u.ExpiresAt <= DateTime.UtcNow && !u.IsDeleted)
                    .ToList();

                if (expiredGuests.Count > 0)
                {
                    _logger.LogInformation($"Found {expiredGuests.Count} expired guest user(s) to delete");

                    foreach (var user in expiredGuests)
                    {
                        user.IsDeleted = true;
                        context.Users.Update(user);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully deleted {expiredGuests.Count} expired guest user(s)");
                }
            }
        }
    }
}
