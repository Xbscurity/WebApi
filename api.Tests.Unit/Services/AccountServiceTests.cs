using api.Dtos.Account;
using api.Models;
using api.Providers.ClientIpProvider;
using api.Providers.CurrentUser;
using api.Repositories;
using api.Services.Account;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.User;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
        private readonly Mock<IClientIpProvider> _clientIpProviderMock = new();
        private readonly Mock<ICurrentUser> _currentUserMock = new();
        private readonly Mock<IUnitOfWorkService> _unitOfWorkMock = new();
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<ErrorOr<string>>>>()))
                .Returns<Func<Task<ErrorOr<string>>>>(action => action());

            _sut = new AccountService(
                _userServiceMock.Object,
                _tokenServiceMock.Object,
                _tokenRepositoryMock.Object,
                Mock.Of<ILogger<AccountService>>(),
                _clientIpProviderMock.Object,
                _currentUserMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetProfileAsync_UserExists_ReturnsUserProfile()
        {
            // Arrange
            var user = AppUserFactory.Create();

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(user.Id);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.GetProfileAsync();

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(user.UserName, result.Value.UserName);
            Assert.Equal(user.Email, result.Value.Email);
            Assert.Equal(user.CreatedAt, result.Value.CreatedAt);
        }

        [Fact]
        public async Task GetProfileAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var userId = "user-999";

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(userId);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.GetProfileAsync();

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.NotFound(userId), result.FirstError);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var userId = "user-999";
            var dto = new ChangePasswordInputDto
            {
                CurrentPassword = "old",
                NewPassword = "new"
            };

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(userId);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.ChangePasswordAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.NotFound(userId), result.FirstError);

            _userServiceMock.Verify(
                x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never);

            _unitOfWorkMock
                .Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<ErrorOr<string>>>>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsInvalidCredentialsError()
        {
            // Arrange
            var user = AppUserFactory.Create();

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = "wrong",
                NewPassword = "new"
            };

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(user.Id);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, input.CurrentPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ChangePasswordAsync(input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Auth.InvalidCredentials(), result.FirstError);

            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            _unitOfWorkMock
                .Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<ErrorOr<string>>>>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_UpdatePasswordFails_ReturnsErrorAndDoesNotRevokeTokens()
        {
            // Arrange
            var user = AppUserFactory.Create();

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = "old",
                NewPassword = "new"
            };

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(user.Id);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, input.CurrentPassword))
                .ReturnsAsync(true);

            _userServiceMock
                .Setup(x => x.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword))
                .ReturnsAsync(Error.Failure());

            // Act
            var result = await _sut.ChangePasswordAsync(input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Error.Failure(), result.FirstError);

            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
            _tokenServiceMock.Verify(
                x => x.CreateRefreshTokenAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_Success_RevokesTokensAndReturnsNewRefreshToken()
        {
            // Arrange
            var user = AppUserFactory.Create();
            const string ip = "127.0.0.1";
            const string newRefreshToken = "new-refresh-token";

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = "old",
                NewPassword = "new"
            };

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(user.Id);

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.CheckPasswordAsync(user, input.CurrentPassword))
                .ReturnsAsync(true);

            _userServiceMock
                .Setup(x => x.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword))
                .ReturnsAsync(Result.Updated);

            _clientIpProviderMock
                .Setup(x => x.GetClientIp())
                .Returns(ip);

            var sequence = new MockSequence();

            _tokenRepositoryMock
                .InSequence(sequence)
                .Setup(x => x.RevokeAllRefreshTokensAsync(user.Id, ip, "Password changed"))
                .Returns(Task.CompletedTask);

            _tokenServiceMock
                .InSequence(sequence)
                .Setup(x => x.CreateRefreshTokenAsync(user.Id))
                .ReturnsAsync(newRefreshToken);

            // Act
            var result = await _sut.ChangePasswordAsync(input);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(newRefreshToken, result.Value);

            _tokenRepositoryMock.Verify(
                x => x.RevokeAllRefreshTokensAsync(user.Id, ip, "Password changed"),
                Times.Once);

            _tokenServiceMock.Verify(
                x => x.CreateRefreshTokenAsync(user.Id),
                Times.Once);
        }
    }
}