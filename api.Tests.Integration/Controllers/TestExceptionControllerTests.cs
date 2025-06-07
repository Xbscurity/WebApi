using api.Helpers;
using api.Tests.Integration.Collections.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class TestExceptionControllerTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _client;
        public TestExceptionControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }
        [Fact]
        public async Task ExceptionFilter_ThrowException_Returns500ServerError()
        {
            await _fixture.ResetCheckpointAsync();
            var response = await _client.GetAsync("/test/testexception/throw");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

            Assert.NotNull(apiResponse);
            Assert.NotNull(apiResponse.Error);
            Assert.Equal("INTERNAL_ERROR", apiResponse.Error.Code);
            Assert.Equal("Server error occured", apiResponse.Error.Message);
        }
    }
}
