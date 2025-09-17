using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Background
{
    /// <summary>
    /// A background service that periodically cleans up expired refresh tokens from the database.
    /// </summary>
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenCleanupService"/> class.
        /// </summary>
        /// <param name="scopeFactory">The factory used to create service scopes for database access.</param>
        /// <param name="logger">The logger used to record informational and error messages.</param>
        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Executes the cleanup task in a continuous loop until cancellation is requested.
        /// The task removes expired refresh tokens from the database every hour.
        /// </summary>
        /// <param name="stoppingToken">A token that signals when the background task should stop.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
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
