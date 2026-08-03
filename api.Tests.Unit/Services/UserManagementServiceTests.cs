using api.Constants;
using api.Dtos.User;
using api.Models;
using api.Queries;
using api.Services.User;
using api.Services.UserManagement;
using api.Specifications;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace api.Tests.Unit.Services
{
    public class UserManagementServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IFusionCache> _cacheMock = new();
        private readonly UserManagementService _sut;

        private readonly DateTimeOffset _now = new(2026, 1, 1, 1, 0, 0, TimeSpan.Zero);
        public UserManagementServiceTests()
        {
            _sut = new UserManagementService(
                _userServiceMock.Object,
                Mock.Of<ILogger<UserManagementService>>(),
                _cacheMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_InvalidSortBy_ReturnsInvalidSortByError()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                SortBy = "notAllowedField",
                Page = 1,
                Size = 10,
            };

            // Act
            var result = await _sut.GetAllUsersAsync(query);

            // Assert
            Assert.True(result.IsError);
            _userServiceMock.Verify(
                s => s.GetAllAsync(It.IsAny<UserManagementPagedSpecification>()),
                Times.Never);
            _userServiceMock.Verify(s => s.CountAsync(), Times.Never);
        }

        [Theory]
        [InlineData("username")]
        [InlineData("email")]
        [InlineData("isBanned")]
        [InlineData("createdat")]
        public async Task GetAllUsersAsync_ValidSortBy_ReturnsPagedItems(string sortBy)
        {
            // Arrange
            var query = new UserManagementQuery
            {
                SortBy = sortBy,
                Page = 1,
                Size = 10,
            };

            var users = new List<UserManagementUserOutputDto>
            {
                new()
                {
                    Id = "user-1",
                    UserName = "alex",
                    Email = "alex@test.com",
                    CreatedAt = _now,
                    IsBanned = false,
                },
            };

            _userServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<UserManagementPagedSpecification>()))
                .ReturnsAsync(users);

            _userServiceMock
                .Setup(s => s.CountAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _sut.GetAllUsersAsync(query);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
            Assert.Equal(users, result.Value.Items);
            Assert.Equal(1, result.Value.Pagination.TotalItems);
        }

        [Theory]
        [InlineData("USERNAME")]
        [InlineData("Email")]
        [InlineData("ISBANNED")]
        [InlineData("CREATEDAT")]
        public async Task GetAllUsersAsync_SortByDifferentCase_IsTreatedAsValid(string sortBy)
        {
            // Arrange
            var query = new UserManagementQuery
            {
                SortBy = sortBy,
                Page = 1,
                Size = 10,
            };

            _userServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<UserManagementPagedSpecification>()))
                .ReturnsAsync(new List<UserManagementUserOutputDto>());

            _userServiceMock
                .Setup(s => s.CountAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _sut.GetAllUsersAsync(query);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");
        }

        [Fact]
        public async Task GetByIdAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            const string userId = "user-999";

            _userServiceMock
                .Setup(s => s.FindByIdAsync(userId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.GetByIdAsync(userId);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public async Task GetByIdAsync_UserExists_ReturnsMappedDto()
        {
            // Arrange
            var user = AppUserFactory.Create();

            _userServiceMock
                .Setup(s => s.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.GetByIdAsync(user.Id);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(user.Id, result.Value.Id);
            Assert.Equal(user.UserName, result.Value.UserName);
            Assert.Equal(user.Email, result.Value.Email);
            Assert.Equal(user.CreatedAt, result.Value.CreatedAt);
            Assert.False(result.Value.IsBanned);
        }

        [Fact]
        public async Task SetBanAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            const string userId = "user-999";
            var input = new BanStatusInputDto
            {
                IsBanned = true
            };

            _userServiceMock
                .Setup(s => s.FindByIdAsync(userId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.SetBanAsync(userId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                Errors.User.NotFound(userId),
                result.FirstError);

            _userServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<AppUser>()),
                Times.Never);

            _cacheMock.Verify(
                c => c.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SetBanAsync_AdminUser_ReturnsAdminBanAttemptError()
        {
            // Arrange
            var user = AppUserFactory.Create();
            var input = new BanStatusInputDto { IsBanned = true };
            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            var result = await _sut.SetBanAsync(user.Id, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(
                Errors.User.AdminBanAttempt(user.Id),
                result.FirstError);

            _userServiceMock.Verify(
                x => x.UpdateAsync(It.IsAny<AppUser>()),
                Times.Never);

            _cacheMock.Verify(
                x => x.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SetBanAsync_UpdateFails_ReturnsError()
        {
            // Arrange
            var user = AppUserFactory.Create();
            var error = Error.Failure("User.UpdateFailed", "Update failed");
            var input = new BanStatusInputDto
            {
                IsBanned = !user.IsBanned
            };

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });

            _userServiceMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(error);

            // Act
            var result = await _sut.SetBanAsync(user.Id, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(error, result.FirstError);

            _cacheMock.Verify(
                x => x.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SetBanAsync_SameBanStatus_ReturnsCurrentStatus(bool isBanned)
        {
            // Arrange
            var user = AppUserFactory.Create(isBanned: isBanned);

            var input = new BanStatusInputDto
            {
                IsBanned = isBanned
            };
            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act
            var result = await _sut.SetBanAsync(user.Id, input);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(isBanned, result.Value.BanStatus);

            _userServiceMock.Verify(
                x => x.UpdateAsync(It.IsAny<AppUser>()),
                Times.Never);

            _cacheMock.Verify(
                x => x.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task SetBanAsync_ValidRequest_UpdatesBanStatus(
            bool initialStatus,
            bool newStatus)
        {
            // Arrange
            var user = AppUserFactory.Create(isBanned: initialStatus);

            var input = new BanStatusInputDto
            {
                IsBanned = newStatus
            };

            _userServiceMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });

            _userServiceMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(Result.Updated);

            _cacheMock
                .Setup(x => x.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            var result = await _sut.SetBanAsync(user.Id, input);

            // Assert
            Assert.False(result.IsError, $"Error code: {result.FirstError.Code}");

            Assert.Equal(newStatus, user.IsBanned);
            Assert.Equal(newStatus, result.Value.BanStatus);

            _userServiceMock.Verify(
                x => x.UpdateAsync(user),
                Times.Once);

            _cacheMock.Verify(
                x => x.RemoveAsync(
                    UserCacheKeys.BanStatus(user.Id),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}