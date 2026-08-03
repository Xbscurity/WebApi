using api.Constants;
using api.Data;
using api.Providers.Time;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Background
{
    /// <summary>
    /// A background service that periodically cleans up expired refresh tokens from the database.
    /// </summary>
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenCleanupService"/> class.
        /// </summary>
        /// <param name="scopeFactory">The factory used to create service scopes for database access.</param>
        /// <param name="timeProvider">Provides the current time.</param>
        /// <param name="logger">The logger used to record informational and error messages.</param>
        public RefreshTokenCleanupService(
            IServiceScopeFactory scopeFactory,
            ITimeProvider timeProvider,
            ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _timeProvider = timeProvider;
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
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            try
            {
                do
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var deletedCount = await db.RefreshTokens
                            .Where(r => r.ExpiresAt < _timeProvider.UtcNow)
                            .ExecuteDeleteAsync(stoppingToken);

                        _logger.LogInformation("Removed expired refresh tokens: {Count}", deletedCount);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(
                            LoggingEvents.Auth.RefreshToken.CleanupError,
                            ex,
                            "Error occurred while cleaning up expired refresh tokens");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}