using api.Dtos.Account;
using api.Dtos.User;
using api.Models;
using api.Tests.Integration.Auth;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class AccountControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;

        public AccountControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetProfile_WithAuthenticatedUser_Returns200WithProfile()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.GetAsync(
                "/api/account/me",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var profile = await response.Content.ReadFromJsonAsync<UserProfileOutputDto>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(profile);
            Assert.Equal("user_user-1@test.com", profile.UserName);
            Assert.Equal("user_user-1@test.com", profile.Email);
        }

        [Fact]
        public async Task ChangePassword_WithValidData_Returns204NoContent()
        {
            // Arrange
            const string userId = "user-1";
            const string currentPassword = "OldPassword123!";
            const string newPassword = "NewPassword123!";

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<AppUser>>();

                var user = new AppUser
                {
                    Id = userId,
                    UserName = "user@test.com",
                    Email = "user@test.com"
                };

                var result = await userManager.CreateAsync(user, currentPassword);

                Assert.True(result.Succeeded);
            }

            using var client = _fixture.Factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.RoleHeader,
                "User");

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.UserIdHeader,
                userId);

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/account/change-password",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_InvalidInput_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/account/change-password",
                (ChangePasswordInputDto?)null,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}