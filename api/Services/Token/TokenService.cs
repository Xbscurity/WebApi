using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace api.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> GenerateAccessTokenAsync(AppUser appUser)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, appUser.Id),
                new Claim(JwtRegisteredClaimNames.Email, appUser.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, appUser.UserName),
            };
            var roles = await _userManager.GetRolesAsync(appUser);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64)).ToLowerInvariant();
        }


        public RefreshToken GenerateRefreshTokenEntity(string plainToken, AppUser user, string? ipAddress)
        {
            var tokenHash = HashToken(plainToken);

            return new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                CreatedByIp = ipAddress ?? "unknown",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            };
        }

        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenResult> TryRefreshTokensAsync(string? refreshTokenPlain, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPlain))
            {
                return new RefreshTokenResult { Error = "No token supplied" };
            }

            var hash = HashToken(refreshTokenPlain);

            var stored = await _context.RefreshTokens
                                  .AsTracking()
                                  .FirstOrDefaultAsync(r => r.TokenHash == hash);

            if (stored == null)
            {
                return new RefreshTokenResult { Error = "Token not found" };
            }

            if (stored.RevokedAt != null)
            {
                if (!string.IsNullOrEmpty(stored.ReplacedByToken))
                {
                    await RevokeDescendantTokensAsync(stored, ipAddress, "Attempted reuse of refresh token");
                }

                return new RefreshTokenResult { Error = "Token already revoked" };
            }

            if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return new RefreshTokenResult { Error = "Token expired" };
            }

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null)
            {
                return new RefreshTokenResult { Error = "User not found" };
            }


            var newAccessToken = await GenerateAccessTokenAsync(user);
            var newPlainRefresh = GenerateRefreshToken();
            var newRefreshEntity = GenerateRefreshTokenEntity(newPlainRefresh, user, ipAddress);

            stored.RevokedAt = DateTimeOffset.UtcNow;
            stored.RevokedByIp = ipAddress;
            stored.Reason = "Replaced by new token";
            stored.ReplacedByToken = newRefreshEntity.TokenHash;

            _context.RefreshTokens.Add(newRefreshEntity);
            await _context.SaveChangesAsync();

            return new RefreshTokenResult
            {
                NewAccessToken = newAccessToken,
                NewRefreshToken = newPlainRefresh,
                ExpiresAt = newRefreshEntity.ExpiresAt,
            };
        }

        public async Task RevokeRefreshTokenAsync(string token, string? ipAddress)
        {
            var tokenHash = HashToken(token);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null);

            if (refreshToken == null)
            {
                return;
            }

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.Reason = "User logout";

            await _context.SaveChangesAsync();
        }

        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private async Task RevokeDescendantTokensAsync(RefreshToken token, string? ipAddress, string reason)
        {
            var current = token;

            while (!string.IsNullOrEmpty(current.ReplacedByToken))
            {
                var next = await _context.RefreshTokens
                    .FirstOrDefaultAsync(r => r.TokenHash == current.ReplacedByToken);

                if (next == null || next.RevokedAt != null)
                {
                    break;
                }

                next.RevokedAt = DateTimeOffset.UtcNow;
                next.RevokedByIp = ipAddress;
                next.Reason = reason;

                current = next;
            }

            await _context.SaveChangesAsync();
        }
    }
}
