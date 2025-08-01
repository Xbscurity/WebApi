using api.Data;
using System;

namespace api.Services.Background
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

                    db.RefreshTokens.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {

                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
