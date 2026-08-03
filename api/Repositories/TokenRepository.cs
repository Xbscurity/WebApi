using api.Data;
using api.Models;
using api.Providers.CurrentUser;
using api.Providers.Time;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    /// <summary>
    /// Default implementation of <see cref="ITokenRepository"/>.
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimeProvider _timeProvider;
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRepository"/> class.
        /// </summary>
        /// <param name="context">
        /// The database context used for token persistence.
        /// </param>
        /// <param name="timeProvider">
        /// The provider used to obtain the current UTC time.
        /// </param>
        /// <param name="currentUser">
        /// The current authenticated user context.
        /// </param>
        public TokenRepository(ApplicationDbContext context, ITimeProvider timeProvider, ICurrentUser currentUser)
        {
            _context = context;
            _timeProvider = timeProvider;
            _currentUser = currentUser;
        }

        /// <inheritdoc/>
        public async Task<RefreshToken?> GetByHashAsync(string hash)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        }

        /// <inheritdoc/>
        public async Task AddAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RevokeByHashAsync(string tokenHash, string? ipAddress, string reason)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null);

            if (refreshToken == null)
            {
                return;
            }

            refreshToken.RevokedAt = _timeProvider.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.Reason = reason;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RevokeAllRefreshTokensAsync(string userId, string? ipAddress, string reason)
        {
            var effectiveIp = ipAddress ?? "unknown";
            await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ExecuteUpdateAsync(
                    setters => setters
                    .SetProperty(rt => rt.RevokedAt, _timeProvider.UtcNow)
                    .SetProperty(rt => rt.RevokedByIp, effectiveIp)
                    .SetProperty(rt => rt.Reason, reason));
        }
    }
}
