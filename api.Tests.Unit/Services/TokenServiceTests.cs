using api.Models;
using api.Options;
using api.Providers.ClientIpProvider;
using api.Providers.Time;
using api.Repositories;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.User;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
namespace api.Tests.Unit.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
        private readonly Mock<ITimeProvider> _timeProviderMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IClientIpProvider> _clientIpProviderMock = new();
        private readonly Mock<IUnitOfWorkService> _unitOfWorkMock = new();
        private readonly JwtOptions _jwtOptions = new()
        {
            SigningKey = "super-secret-signing-key-used-only-for-unit-tests-1234567890-ABC",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 15,
        };

        private readonly RefreshTokenOptions _refreshTokenOptions = new()
        {
            Length = 32,
            ExpirationDays = 7,
        };

        private readonly DateTimeOffset _now = new(2026, 1, 1, 1, 0, 0, TimeSpan.Zero);

        private readonly TokenService _sut;

        private const string RoleClaimType = "role";

        public TokenServiceTests()
        {
            _timeProviderMock.Setup(x => x.UtcNow).Returns(_now);

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<ErrorOr<RefreshTokenDto>>>>()))
                .Returns<Func<Task<ErrorOr<RefreshTokenDto>>>>(action => action());

            _sut = new TokenService(
                Microsoft.Extensions.Options.Options.Create(_jwtOptions),
                _tokenRepositoryMock.Object,
                _timeProviderMock.Object,
                _userServiceMock.Object,
                Microsoft.Extensions.Options.Options.Create(_refreshTokenOptions),
                Mock.Of<ILogger<TokenService>>(),
                _clientIpProviderMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ValidUser_ReturnsTokenWithCorrectClaims()
        {
            // Arrange
            var user = AppUserFactory.Create();
            var roles = new List<string> { "User", "Admin" };

            _userServiceMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var token = await _sut.GenerateAccessTokenAsync(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(user.Id, jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(_jwtOptions.Issuer, jwt.Issuer);
            Assert.Contains(_jwtOptions.Audience, jwt.Audiences);
            Assert.Equal(2, jwt.Claims.Count(c => c.Type == RoleClaimType));
            Assert.Contains(jwt.Claims, c => c.Type == RoleClaimType && c.Value == "User");
            Assert.Contains(jwt.Claims, c => c.Type == RoleClaimType && c.Value == "Admin");
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_TokenExpiration_SetsCorrectTimeClaims()
        {
            // Arrange
            var user = AppUserFactory.Create();
            _userServiceMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // Act
            var token = await _sut.GenerateAccessTokenAsync(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var expectedExpiry = _now.AddMinutes(_jwtOptions.ExpirationMinutes);
            Assert.Equal(expectedExpiry, jwt.ValidTo, TimeSpan.FromSeconds(1));
            Assert.Equal(_now.UtcDateTime, jwt.IssuedAt, TimeSpan.FromSeconds(1));
            Assert.Equal(_now, jwt.ValidFrom, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_NoRoles_ContainsNoRoleClaims()
        {
            // Arrange
            var user = AppUserFactory.Create();
            _userServiceMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // Act
            var token = await _sut.GenerateAccessTokenAsync(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == RoleClaimType);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ValidUserId_CreatesRefreshTokenEntity()
        {
            // Arrange
            const string userId = "user-id";
            const string ip = "127.0.0.1";

            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns(ip);

            RefreshToken? savedToken = null;

            _tokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => savedToken = t)
                .Returns(Task.CompletedTask);

            // Act
            var plainToken = await _sut.CreateRefreshTokenAsync(userId);

            // Assert
            Assert.NotNull(savedToken);

            Assert.Equal(userId, savedToken!.UserId);
            Assert.Equal(ip, savedToken.CreatedByIp);
            Assert.Equal(_now, savedToken.CreatedAt);
            Assert.Equal(
                _now.AddDays(_refreshTokenOptions.ExpirationDays),
                savedToken.ExpiresAt);

            Assert.False(string.IsNullOrWhiteSpace(savedToken.TokenHash));
            Assert.NotEqual(plainToken, savedToken.TokenHash);

            _tokenRepositoryMock.Verify(
                x => x.AddAsync(savedToken),
                Times.Once);
        }

        [Fact]
        public async Task ValidateStoredTokenAsync_EmptyToken_ReturnsRefreshTokenNotSuppliedError()
        {
            // Arrange
            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns("127.0.0.1");

            // Act
            var result = await _sut.ValidateStoredTokenAsync(null);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                Errors.Auth.RefreshTokenNotSupplied(),
                result.FirstError);

            _tokenRepositoryMock.Verify(
                x => x.GetByHashAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ValidateStoredTokenAsync_TokenNotFound_ReturnsRefreshTokenNotFoundError()
        {
            // Arrange
            const string token = "refresh-token";

            _tokenRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _sut.ValidateStoredTokenAsync(token);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                Errors.Auth.RefreshTokenNotFound(),
                result.FirstError);
        }

        [Fact]
        public async Task ValidateStoredTokenAsync_RevokedToken_RevokesAllTokensAndReturnsError()
        {
            // Arrange
            var storedToken = new RefreshToken
            {
                UserId = "user-id",
                TokenHash = "hash",
                RevokedAt = _now,
            };

            _tokenRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(storedToken);

            // Act
            var result = await _sut.ValidateStoredTokenAsync("token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(
                Errors.Auth.RefreshTokenAlreadyRevoked(),
                result.FirstError);


            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(
                    storedToken.UserId,
                    It.IsAny<string>(),
                    "Attempted reuse of refresh token"),
                Times.Once);
        }

        [Fact]
        public async Task ValidateStoredTokenAsync_ExpiredToken_ReturnsExpiredError()
        {
            // Arrange
            var storedToken = new RefreshToken
            {
                UserId = "user-id",
                TokenHash = "hash",
                ExpiresAt = _now.AddDays(-1)
            };

            _tokenRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(storedToken);


            // Act
            var result = await _sut.ValidateStoredTokenAsync("token");


            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                Errors.Auth.RefreshTokenExpired(),
                result.FirstError);
        }

        [Fact]
        public async Task ValidateStoredTokenAsync_ValidToken_ReturnsStoredToken()
        {
            // Arrange
            var storedToken = new RefreshToken
            {
                UserId = "user-id",
                TokenHash = "hash",
                ExpiresAt = _now.AddDays(1),
            };

            _tokenRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(storedToken);

            // Act
            var result = await _sut.ValidateStoredTokenAsync("token");


            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");

            Assert.Equal(
                storedToken,
                result.Value);
        }

        [Fact]
        public async Task RotateTokensAsync_MismatchedUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = AppUserFactory.Create(id: "user-1");
            var token = new RefreshToken
            {
                UserId = "other-user-id",
                TokenHash = "hash",
            };

            // Act
            var action = () => _sut.RotateTokensAsync(user, token);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(action);

            _unitOfWorkMock.Verify(
                x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task<ErrorOr<RefreshTokenDto>>>>()),
                Times.Never);

            _tokenRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RotateTokensAsync_ValidToken_RotatesRefreshToken()
        {
            // Arrange
            const string ip = "127.0.0.1";

            var user = AppUserFactory.Create();

            var storedToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = "old-token-hash",
                ExpiresAt = _now.AddDays(7),
            };

            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns(ip);

            _userServiceMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _sut.RotateTokensAsync(user, storedToken);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");

            Assert.Equal(_now, storedToken.RevokedAt);
            Assert.Equal(ip, storedToken.RevokedByIp);
            Assert.Equal("Replaced by new token", storedToken.Reason);
            Assert.False(string.IsNullOrWhiteSpace(storedToken.ReplacedByToken));
            Assert.NotEqual(result.Value.RefreshToken, storedToken.ReplacedByToken);

            Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));

            _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _unitOfWorkMock.Verify(
                x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task<ErrorOr<RefreshTokenDto>>>>()),
                Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ValidToken_RevokesTokenByHash()
        {
            // Arrange
            const string token = "refresh-token";
            const string ip = "127.0.0.1";
            const string reason = "User logout";

            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns(ip);

            // Act
            await _sut.RevokeRefreshTokenAsync(token, reason);

            // Assert
            _tokenRepositoryMock.Verify(
                x => x.RevokeByHashAsync(
                    It.Is<string>(hash => hash != token && !string.IsNullOrEmpty(hash)),
                    ip,
                    reason),
                Times.Once);
        }
    }
}