using api.Helpers;
using api.Models;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class CategoryControllerTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _client;

        public CategoryControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }
        [Fact]
        public async Task GetAll_SortedByIdSecondPage_ReturnsFirstTwoCategories()
        {
            // Arrange
            await _fixture.ResetCheckpointAsync();
            var firstCategoryDto = new CategoryInputDto { Name = "CCC" };
            var secondCategoryDto = new CategoryInputDto { Name = "AAA" };
            var thirdCategoryDto = new CategoryInputDto { Name = "BBB" };

            var postResponse1 = await _client.PostAsJsonAsync("/api/categories", firstCategoryDto);
            postResponse1.EnsureSuccessStatusCode();

            var postResponse2 = await _client.PostAsJsonAsync("/api/categories", secondCategoryDto);
            postResponse2.EnsureSuccessStatusCode();

            var postResponse3 = await _client.PostAsJsonAsync("/api/categories", thirdCategoryDto);
            postResponse3.EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync("/api/categories?page=1&size=2&sortBy=name");
            response.EnsureSuccessStatusCode();

            // Assert
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Category>>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("AAA", result.Data[0].Name);
            Assert.Equal("BBB", result.Data[1].Name);
        }
        [Fact]
        public async Task CreateAndGetById_ValidCategory_ReturnsCreatedCategory()
        {
            await _fixture.ResetCheckpointAsync();

            var firstCategory = new CategoryInputDto { Name = "AAA" };
            var postResponse1 = await _client.PostAsJsonAsync("/api/categories", firstCategory);
            postResponse1.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/categories/1");
            getResponse.EnsureSuccessStatusCode();

            var result = await getResponse.Content.ReadFromJsonAsync<ApiResponse<Category>>();
            Assert.NotNull(result);
            Assert.Equal("AAA", result.Data.Name);
        }
        [Fact]
        public async Task Update_ExistingCategory_ReturnsUpdatedCategory()
        {
            await _fixture.ResetCheckpointAsync();
            var createDto = new CategoryInputDto { Name = "Original" };
            var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
            createResponse.EnsureSuccessStatusCode();

            var updateDto = new CategoryInputDto { Name = "Updated" };

            var updateResponse = await _client.PutAsJsonAsync("/api/categories/1", updateDto);
            updateResponse.EnsureSuccessStatusCode();

            var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<Category>>();
            Assert.Equal("Updated", updated.Data.Name);
        }
        [Fact]
        public async Task Delete_ExistingCategory_ReturnsNotFoundAfterDeletedCategory()
        {
            await _fixture.ResetCheckpointAsync();

            var categoryDto = new CategoryInputDto { Name = "ToBeDeleted" };
            var postResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
            postResponse.EnsureSuccessStatusCode();

            var getBeforeDeleteResponse = await _client.GetAsync("/api/categories/1");
            getBeforeDeleteResponse.EnsureSuccessStatusCode();

            var deleteResponse = await _client.DeleteAsync("/api/categories/1");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/categories/1");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
        [Fact]
        public async Task Create_InvalidShortNameCategory_ReturnsValidationError()
        {
            await _fixture.ResetCheckpointAsync();

            var invalidCategory = new CategoryInputDto { Name = "A" };
            var response = await _client.PostAsJsonAsync("/api/categories", invalidCategory);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal("VALIDATION_ERROR", apiResponse.Error.Code);
            Assert.Equal("Validation failed", apiResponse.Error.Message);
        }
    }
}
