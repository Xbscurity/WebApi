using api.Models;
using api.Options;
using api.Providers.CurrentUser;
using api.Services.User;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace api.Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _userManagerMock = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<AppUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
                null, null, null, null);

            _sut = new UserService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                Mock.Of<IFusionCache>(),
                Mock.Of<ICurrentUser>(),
                Microsoft.Extensions.Options.Options.Create(new CacheOptions { BanUserTtl = TimeSpan.FromMinutes(5) }),
                Mock.Of<ILogger<UserService>>());
        }

        [Fact]
        public async Task CheckPasswordSignInAsync_AccountLockedOut_ReturnsAccountLockedOutError()
        {
            // Arrange
            var user = AppUserFactory.Create();

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, "wrong-password", true))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _sut.CheckPasswordSignInAsync(user, "wrong-password");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Auth.AccountLockedOut(), result.FirstError);
        }

        [Fact]
        public async Task CheckPasswordSignInAsync_WrongPassword_ReturnsInvalidCredentialsError()
        {
            // Arrange
            var user = AppUserFactory.Create();

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, "wrong-password", true))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _sut.CheckPasswordSignInAsync(user, "wrong-password");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Auth.InvalidCredentials(), result.FirstError);
        }

        [Fact]
        public async Task CheckPasswordSignInAsync_CorrectPassword_ReturnsSuccess()
        {
            // Arrange
            var user = AppUserFactory.Create();

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, "correct-password", true))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _sut.CheckPasswordSignInAsync(user, "correct-password");

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
        }

        [Fact]
        public async Task CreateAsync_Succeeds_ReturnsCreated()
        {
            var user = AppUserFactory.Create();

            _userManagerMock
                .Setup(x => x.CreateAsync(user, "password"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.CreateAsync(user, "password");

            Assert.True(result.IsSuccess);
            Assert.Equal(Result.Created, result.Value);
        }

        [Fact]
        public async Task CreateAsync_Fails_ReturnsMappedErrors()
        {
            var user = AppUserFactory.Create();
            var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Username already taken. " };

            _userManagerMock
                .Setup(x => x.CreateAsync(user, "password"))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _sut.CreateAsync(user, "password");

            Assert.True(result.IsError);
        }

        [Fact]
        public async Task UpdateAsync_Succeeds_ReturnsUpdated()
        {
            var user = AppUserFactory.Create();

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.UpdateAsync(user);

            Assert.True(result.IsSuccess);
            Assert.Equal(Result.Updated, result.Value);
        }

        [Fact]
        public async Task UpdateAsync_Fails_ReturnsMappedErrors()
        {
            var user = AppUserFactory.Create();
            var identityError = new IdentityError { Code = "UpdateFailed", Description = "Update fails." };

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _sut.UpdateAsync(user);

            Assert.True(result.IsError);
        }

        [Fact]
        public async Task AddToRoleAsync_Succeeds_ReturnsSuccess()
        {
            var user = AppUserFactory.Create();

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "user"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.AddToRoleAsync(user, "user");

            Assert.True(result.IsSuccess);
            Assert.Equal(Result.Success, result.Value);
        }

        [Fact]
        public async Task AddToRoleAsync_Fails_ReturnsMappedErrors()
        {
            var user = AppUserFactory.Create();
            var identityError = new IdentityError { Code = "RoleAssignmentFailed", Description = "Role assignment fails. " };

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "invalid"))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _sut.AddToRoleAsync(user, "invalid");

            Assert.True(result.IsError);
        }

        [Fact]
        public async Task ChangePasswordAsync_Succeeds_ReturnsUpdated()
        {
            var user = AppUserFactory.Create();

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "current", "new"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.ChangePasswordAsync(user, "current", "new");

            Assert.True(result.IsSuccess);
            Assert.Equal(Result.Updated, result.Value);
        }

        [Fact]
        public async Task ChangePasswordAsync_Fails_ReturnsMappedErrors()
        {
            var user = AppUserFactory.Create();
            var identityError = new IdentityError { Code = "InvalidPassword", Description = "Invalid password. " };

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "invalid", "new"))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _sut.ChangePasswordAsync(user, "invalid", "new");

            Assert.True(result.IsError);
        }
    }
}