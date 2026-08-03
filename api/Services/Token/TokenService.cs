using api.Constants;
using api.Models;
using api.Options;
using api.Providers.ClientIpProvider;
using api.Providers.Time;
using api.Repositories;
using api.Services.UnitOfWork;
using api.Services.User;
using ErrorOr;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace api.Services.Token
{
    /// <summary>
    /// Default implementation of <see cref="ITokenService"/>.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ITokenRepository _tokenRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IUserService _userService;
        private readonly RefreshTokenOptions _refreshTokenOptions;
        private readonly ILogger<TokenService> _logger;
        private readonly IClientIpProvider _clientIpProvider;
        private readonly IUnitOfWorkService _unitOfWork;
        private readonly SymmetricSecurityKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="jwtOptions">JWT configuration options.</param>
        /// <param name="tokenRepository">The repository used to store refresh tokens.</param>
        /// <param name="timeProvider">Provides the current time.</param>
        /// <param name="userService">Provides user-related operations.</param>
        /// <param name="refreshTokenOptions">Refresh token configuration options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="clientIpProvider">Provides the client IP address.</param>
        /// <param name="unitOfWork">Provides transactional persistence for repository operations.</param>
        public TokenService(
            IOptions<JwtOptions> jwtOptions,
            ITokenRepository tokenRepository,
            ITimeProvider timeProvider,
            IUserService userService,
            IOptions<RefreshTokenOptions> refreshTokenOptions,
            ILogger<TokenService> logger,
            IClientIpProvider clientIpProvider,
            IUnitOfWorkService unitOfWork)
        {
            _jwtOptions = jwtOptions.Value;
            _tokenRepository = tokenRepository;
            _timeProvider = timeProvider;
            _userService = userService;
            _refreshTokenOptions = refreshTokenOptions.Value;
            _logger = logger;
            _clientIpProvider = clientIpProvider;
            _unitOfWork = unitOfWork;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAccessTokenAsync(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            };
            var roles = await _userService.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = _timeProvider.UtcNow.UtcDateTime,
                IssuedAt = _timeProvider.UtcNow.UtcDateTime,
                Expires = _timeProvider.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes).UtcDateTime,
                SigningCredentials = creds,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation(
                "Access token successfully generated");

            return tokenHandler.WriteToken(token);
        }

        /// <inheritdoc/>
        public async Task<string> CreateRefreshTokenAsync(string userId)
        {
            var plainToken = Convert
                .ToHexString(RandomNumberGenerator
                .GetBytes(_refreshTokenOptions.Length))
                .ToLowerInvariant();
            var tokenHash = HashToken(plainToken);
            var ipAddress = _clientIpProvider.GetClientIp() ?? "unknown";

            _logger.LogInformation(
                "Creating refresh token entity, IP={IP}",
                ipAddress);

            var refreshToken = new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = userId,
                CreatedByIp = ipAddress,
                CreatedAt = _timeProvider.UtcNow,
                ExpiresAt = _timeProvider.UtcNow.AddDays(_refreshTokenOptions.ExpirationDays),
            };
            await _tokenRepository.AddAsync(refreshToken);
            return plainToken;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<RefreshToken>> ValidateStoredTokenAsync(string? refreshTokenPlain)
        {
            var ipAddress = _clientIpProvider.GetClientIp() ?? "unknown";
            if (string.IsNullOrWhiteSpace(refreshTokenPlain))
            {
                _logger.LogWarning(
                    LoggingEvents.Auth.RefreshToken.NotSupplied,
                    "Refresh token not supplied. IP={IP}",
                    ipAddress);
                return Errors.Auth.RefreshTokenNotSupplied();
            }

            var hash = HashToken(refreshTokenPlain);

            var stored = await _tokenRepository.GetByHashAsync(hash);
            if (stored == null)
            {
                _logger.LogWarning(
                    LoggingEvents.Auth.RefreshToken.NotFound,
                    "Refresh token not found. IP={IP}",
                    ipAddress);
                return Errors.Auth.RefreshTokenNotFound();
            }

            if (stored.IsRevoked)
            {
                _logger.LogWarning(
                    LoggingEvents.Auth.RefreshToken.ReuseAttempt,
                    "Refresh token reuse detected.IP={IP}",
                    ipAddress);

                await _tokenRepository
                    .RevokeAllRefreshTokensAsync(stored.UserId, ipAddress, "Attempted reuse of refresh token");
                return Errors.Auth.RefreshTokenAlreadyRevoked();
            }

            if (stored.ExpiresAt <= _timeProvider.UtcNow)
            {
                _logger.LogInformation(
                    "Expired refresh token used");
                return Errors.Auth.RefreshTokenExpired();
            }

            return stored;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<RefreshTokenDto>> RotateTokensAsync(AppUser user, RefreshToken stored)
        {
            if (stored.UserId != user.Id)
            {
                throw new InvalidOperationException(
                    $"Refresh token rotation was attempted with mismatched user and token ownership. " +
                    $"User.Id={user.Id}, RefreshToken.UserId={stored.UserId}.");
            }

            return await _unitOfWork.ExecuteInTransactionAsync<RefreshTokenDto>(async () =>
            {
                var newRefreshToken = await CreateRefreshTokenAsync(user.Id);

                var ipAddress = _clientIpProvider.GetClientIp() ?? "unknown";

                stored.RevokedAt = _timeProvider.UtcNow;
                stored.RevokedByIp = ipAddress;
                stored.Reason = "Replaced by new token";
                stored.ReplacedByToken = HashToken(newRefreshToken);

                await _tokenRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Refresh token successfully rotated");

                var newAccessToken = await GenerateAccessTokenAsync(user);

                return new RefreshTokenDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                };
            });
        }

        /// <inheritdoc/>
        public async Task RevokeRefreshTokenAsync(string token, string reason)
        {
            var tokenHash = HashToken(token);
            var ipAddress = _clientIpProvider.GetClientIp() ?? "unknown";
            _logger.LogInformation(
                "Revoking refresh token. IP={Ip}, Reason={Reason}",
                ipAddress,
                reason);
            await _tokenRepository.RevokeByHashAsync(tokenHash, ipAddress, reason);
        }

        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}