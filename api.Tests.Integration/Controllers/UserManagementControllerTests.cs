using api.Constants;
using api.Data;
using api.Dtos.User;
using api.Models;
using api.Services.Shared;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class UserManagementControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;

        public UserManagementControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync() => await _fixture.ResetCheckpointAsync();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetAllUsers_WithExistingUsers_Returns200WithPagedList()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.AddRange(
                    new AppUser { Id = "user-1", UserName = "alice", Email = "alice@test.com" },
                    new AppUser { Id = "user-2", UserName = "bob", Email = "bob@test.com" });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<UserManagementUserOutputDto>>(
                "/api/users?page=1&size=10",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(3, response.Items.Count);
        }

        [Fact]
        public async Task GetAllUsers_WhenSortByIsInvalid_Returns422UnprocessableEntity()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                "/api/users?sortby=invalid",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithExistingUser_Returns200WithUser()
        {
            // Arrange
            const string targetUserId = "user-1";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = targetUserId, UserName = "alice", Email = "alice@test.com" });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<UserManagementUserOutputDto>(
                $"/api/users/{targetUserId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(targetUserId, response.Id);
        }

        [Fact]
        public async Task GetById_WhenUserDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                "/api/users/non-existent-user",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetBanStatus_WithValidData_Returns200AndUpdatesBanStatus()
        {
            // Arrange
            const string targetUserId = "user-1";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = targetUserId, UserName = "alice", Email = "alice@test.com", IsBanned = false });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new BanStatusInputDto { IsBanned = true };

            // Act
            var response = await client.PostAsJsonAsync(
                $"/api/users/{targetUserId}/ban-status",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<BanStatusOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.True(result.BanStatus);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await db2.Users.FindAsync(
                [targetUserId], TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.True(saved.IsBanned);
        }

        [Fact]
        public async Task SetBanStatus_WhenUserDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");
            var dto = new BanStatusInputDto { IsBanned = true };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/users/non-existent-user/ban-status",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetBanStatus_WhenTargetIsAdmin_Returns403Forbidden()
        {
            // Arrange
            const string targetAdminId = "admin-2";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var targetAdmin = new AppUser
                {
                    Id = targetAdminId,
                    UserName = "admin2@test.com",
                    Email = "admin2@test.com",
                    IsBanned = false
                };

                var createResult = await userManager.CreateAsync(targetAdmin);
                Assert.True(createResult.Succeeded);

                var addRoleResult = await userManager.AddToRoleAsync(targetAdmin, Roles.Admin);
                Assert.True(addRoleResult.Succeeded);
            }

            var dto = new BanStatusInputDto { IsBanned = true };

            // Act
            var response = await client.PostAsJsonAsync(
                $"/api/users/{targetAdminId}/ban-status",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouched = await db2.Users.FindAsync(
                [targetAdminId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouched);
            Assert.False(untouched.IsBanned);
        }
    }
}