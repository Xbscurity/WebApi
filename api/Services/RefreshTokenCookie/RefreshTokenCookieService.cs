using api.Constants;
using api.Options;
using api.Providers.Time;
using Microsoft.Extensions.Options;

namespace api.Services.RefreshTokenCookie
{
    /// <summary>
    /// Default implementation of <see cref="IRefreshTokenCookieService"/>.
    /// </summary>
    public class RefreshTokenCookieService : IRefreshTokenCookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private readonly RefreshTokenOptions _options;
        private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenCookieService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        /// <param name="env">Provides information about the hosting environment.</param>
        /// <param name="options">Configuration options for refresh tokens.</param>
        /// <param name="timeProvider">Abstraction for retrieving the current time.</param>
        public RefreshTokenCookieService(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            IOptions<RefreshTokenOptions> options,
            ITimeProvider timeProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        /// <inheritdoc/>
        public void Set(string token)
        {
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(
                CookieNames.RefreshToken,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    Expires = _timeProvider.UtcNow.AddDays(_options.ExpirationDays),
                });
        }
    }
}
