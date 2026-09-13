using api.Tests.Integration.Collections.Fixtures;
using System.Net;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class TestExceptionControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        public TestExceptionControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync() => await _fixture.ResetCheckpointAsync();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task ExceptionFilter_ThrowException_Returns500ServerError()
        {
            // Arrange
            using var client = _fixture.CreateAnonymousClient();

            // Act
            var response = await client.GetAsync(
                "/test/testexception/throw", TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
