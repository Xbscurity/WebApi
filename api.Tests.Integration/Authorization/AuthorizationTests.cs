using api.Dtos.User;
using api.Tests.Integration.Collections.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Authorization
{
    [Collection("IntegrationTestCollection")]
    public class AuthorizationTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        public AuthorizationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task ProtectedEndpoint_WhenAnonymous_Returns401()
        {
            // Arrange
            using var client = _fixture.CreateAnonymousClient();

            // Act
            var response = await client.GetAsync(
                "/api/categories",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WhenUserIsBanned_Returns403()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId, isBanned: true);

            // Act
            var response = await client.GetAsync(
                "/api/financial-transactions",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AnonymousEndpoint_WhenAnonymous_IsAccessible()
        {
            // Arrange
            using var client = _fixture.CreateAnonymousClient();
            var input = new RegisterInputDto
            {
                Email = "test@test.com",
                UserName = "test",
                Password = "Password123!"
            };
            // Act
            var response = await client.PostAsJsonAsync(
                "/api/auth/register",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            var raw = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AdminEndpoint_WhenUserIsNotAdmin_Returns403Forbidden()
        {
            // Arrange
            const string userId = "user-2";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.GetAsync(
                "/api/admin/categories",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}