using api.Tests.Integration.Collections.Fixtures;
using System.Net;

namespace api.Tests.Integration.Category
{
    [Collection("IntegrationTestCollection")]
    public class UsersApiTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _client;

        public UsersApiTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            await _fixture.ResetCheckpointAsync();

            var response = await _client.GetAsync("/api/categories/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task GetUsers_ReturnsOk()
        {
            await _fixture.ResetCheckpointAsync();
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
