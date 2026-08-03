using api.Constants;
using api.Dtos.Account;
using api.Dtos.User;
using api.Models;
using api.Providers.ClientIpProvider;
using api.Repositories;
using api.Services.Auth;
using api.Services.Categories;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.User;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly Mock<IUnitOfWorkService> _unitOfWorkMock = new();
        private readonly Mock<IClientIpProvider> _clientIpProviderMock = new();

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<ErrorOr<AuthResult>>>>()))
                .Returns<Func<Task<ErrorOr<AuthResult>>>>(action => action());

            _sut = new AuthService(
                _userServiceMock.Object,
                _tokenServiceMock.Object,
                _tokenRepositoryMock.Object,
                _categoryServiceMock.Object,
                Mock.Of<ILogger<AuthService>>(),
                _unitOfWorkMock.Object,
                _clientIpProviderMock.Object
                );
        }

        [Fact]
        public async Task RegisterAsync_UserCreationFails_ReturnsErrorAndDoesNotAssignRole()
        {
            // Arrange
            var dto = new RegisterInputDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
            };
            var expectedError = Error.Validation("User.Duplicate", "Username already taken");

            _userServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), dto.Password))
                .ReturnsAsync(expectedError);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(expectedError, result.FirstError);

            _userServiceMock.Verify(
                x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never);
            _categoryServiceMock.Verify(
                x => x.CreateInitialCategoriesForUserAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_RoleAssignmentFails_ReturnsErrorAndDoesNotCreateCategories()
        {
            // Arrange
            var dto = new RegisterInputDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
            };
            var expectedError = Error.Failure("User.RoleAssignmentFailed", "Could not assign role");

            _userServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), dto.Password))
                .ReturnsAsync(Result.Created);
            _userServiceMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), Roles.User))
                .ReturnsAsync(expectedError);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(expectedError, result.FirstError);

            _categoryServiceMock.Verify(
                x => x.CreateInitialCategoriesForUserAsync(It.IsAny<string>()),
                Times.Never);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessTokenAsync(It.IsAny<AppUser>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_Success_CreatesUserAssignsRoleAndReturnsAuthResult()
        {
            // Arrange
            var dto = new RegisterInputDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
            };
            const string userId = "user-1";
            const string accessToken = "access-token";
            const string refreshToken = "refresh-token";

            AppUser? capturedUser = null;

            _userServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), dto.Password))
                .Callback<AppUser, string>((user, _) =>
                {
                    user.Id = userId;
                    capturedUser = user;
                })
                .ReturnsAsync(Result.Created);

            _userServiceMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), Roles.User))
                .ReturnsAsync(Result.Success);

            _categoryServiceMock
                .Setup(x => x.CreateInitialCategoriesForUserAsync(userId))
                .Returns(Task.CompletedTask);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessTokenAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(accessToken);
            _tokenServiceMock
                .Setup(x => x.CreateRefreshTokenAsync(userId))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(dto.UserName, result.Value.UserName);
            Assert.Equal(dto.Email, result.Value.Email);
            Assert.Equal(accessToken, result.Value.AccessToken);
            Assert.Equal(refreshToken, result.Value.RefreshToken);

            Assert.NotNull(capturedUser);
            Assert.Equal(dto.UserName, capturedUser.UserName);
            Assert.Equal(dto.Email, capturedUser.Email);

            _categoryServiceMock.Verify(
                x => x.CreateInitialCategoriesForUserAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsInvalidCredentialsError()
        {
            // Arrange
            var dto = new LoginInputDto
            {
                UserName = "user-999",
                Password = "any"
            };

            _userServiceMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Auth.InvalidCredentials(), result.FirstError);

            _userServiceMock.Verify(
                x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsInvalidCredentialsError()
        {
            // Arrange
            var user = AppUserFactory.Create();
            var dto = new LoginInputDto
            {
                UserName = user.UserName!,
                Password = "wrong"
            };

            _userServiceMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync(user);
            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Auth.InvalidCredentials(), result.FirstError);

            _tokenServiceMock.Verify(
                x => x.GenerateAccessTokenAsync(It.IsAny<AppUser>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UserBanned_ReturnsBannedError()
        {
            // Arrange
            var user = AppUserFactory.Create(isBanned: true);

            var dto = new LoginInputDto
            {
                UserName = user.UserName!,
                Password = "any"
            };

            _userServiceMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.Banned(), result.FirstError);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResult()
        {
            // Arrange
            var user = AppUserFactory.Create();
            var dto = new LoginInputDto
            {
                UserName = user.UserName!,
                Password = "correct"
            };
            const string accessToken = "access-token";
            const string refreshToken = "refresh-token";

            _userServiceMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync(user);
            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _tokenServiceMock
                .Setup(x => x.GenerateAccessTokenAsync(user))
                .ReturnsAsync(accessToken);
            _tokenServiceMock
                .Setup(x => x.CreateRefreshTokenAsync(user.Id))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(user.UserName, result.Value.UserName);
            Assert.Equal(user.Email, result.Value.Email);
            Assert.Equal(accessToken, result.Value.AccessToken);
            Assert.Equal(refreshToken, result.Value.RefreshToken);
        }

        [Fact]
        public async Task RefreshSessionAsync_InvalidToken_ReturnsError()
        {
            // Arrange
            const string refreshToken = "invalid-token";
            var expectedError = Error.Failure("Token.Invalid", "Invalid or expired refresh token");

            _tokenServiceMock
                .Setup(x => x.ValidateStoredTokenAsync(refreshToken))
                .ReturnsAsync(expectedError);

            // Act
            var result = await _sut.RefreshSessionAsync(refreshToken);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(expectedError, result.FirstError);

            _userServiceMock.Verify(
                x => x.FindByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshSessionAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            const string refreshToken = "valid-token";
            var storedToken = CreateRefreshToken();

            _tokenServiceMock
                .Setup(x => x.ValidateStoredTokenAsync(refreshToken))
                .ReturnsAsync(storedToken);
            _userServiceMock
                .Setup(x => x.FindByIdAsync(storedToken.UserId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.RefreshSessionAsync(refreshToken);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.NotFound(storedToken.UserId), result.FirstError);

            _tokenServiceMock.Verify(
                x => x.RotateTokensAsync(It.IsAny<AppUser>(), It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshSessionAsync_UserBanned_RevokesAllTokensAndReturnsBannedError()
        {
            // Arrange
            const string refreshToken = "valid-token";
            var user = AppUserFactory.Create(isBanned: true);
            var storedToken = CreateRefreshToken(userId: user.Id);
            const string ip = "127.0.0.1";

            _tokenServiceMock
                .Setup(x => x.ValidateStoredTokenAsync(refreshToken))
                .ReturnsAsync(storedToken);
            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns(ip);
            // Act
            var result = await _sut.RefreshSessionAsync(refreshToken);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.Banned(), result.FirstError);

            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(user.Id, ip, "User banned"),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.RotateTokensAsync(It.IsAny<AppUser>(), It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshSessionAsync_RotationFails_ReturnsError()
        {
            // Arrange
            const string refreshToken = "valid-token";
            var user = AppUserFactory.Create();
            var storedToken = CreateRefreshToken(userId: user.Id);

            var expectedError = Error.Failure("Token.RotationFailed", "Could not rotate tokens");

            _tokenServiceMock
                .Setup(x => x.ValidateStoredTokenAsync(refreshToken))
                .ReturnsAsync(storedToken);
            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _tokenServiceMock
                .Setup(x => x.RotateTokensAsync(user, storedToken))
                .ReturnsAsync(expectedError);

            // Act
            var result = await _sut.RefreshSessionAsync(refreshToken);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(expectedError, result.FirstError);
        }

        [Fact]
        public async Task RefreshSessionAsync_ValidToken_ReturnsRotatedTokens()
        {
            // Arrange
            const string refreshToken = "valid-token";
            var user = AppUserFactory.Create();
            var storedToken = CreateRefreshToken(userId: user.Id);

            var expectedResult = new RefreshTokenDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
            };

            _tokenServiceMock
                .Setup(x => x.ValidateStoredTokenAsync(refreshToken))
                .ReturnsAsync(storedToken);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.RotateTokensAsync(user, storedToken))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.RefreshSessionAsync(refreshToken);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(expectedResult.AccessToken, result.Value.AccessToken);
            Assert.Equal(expectedResult.RefreshToken, result.Value.RefreshToken);

            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        private RefreshToken CreateRefreshToken(
            string tokenHash = "hash",
            string userId = "user-1")
        {
            return new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = userId
            };
        }
    }
}