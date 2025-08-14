using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Background
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expired = db.RefreshTokens
                        .Where(r => r.ExpiresAt < DateTimeOffset.UtcNow);

                    var deletedCount = await expired.ExecuteDeleteAsync();

                    if (deletedCount > 0)
                    {
                        _logger.LogInformation("Removed {Count} expired refresh tokens", deletedCount);
                    }
                    else
                    {
                        _logger.LogDebug("No expired refresh tokens found to remove");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired refresh tokens");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
