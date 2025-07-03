using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.Tests.Integration.Collections.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class TransactionControllerTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly HttpClient _client;
        public TransactionControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }
        [Fact]
        public async Task GetAll_SortedByAmountSecondPage_ReturnsFirstTwoFinancialTransactions()
        {
            // Arrange
            await _fixture.ResetCheckpointAsync();
            var categoryDto = new CategoryInputDto { Name = "abc" };

            var categoryPostResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
            categoryPostResponse.EnsureSuccessStatusCode();

            var financialTransactionDto2 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 200,
                Comment = "test",
            };
            var financialTransactionPostResponse2 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto2);
            financialTransactionPostResponse2.EnsureSuccessStatusCode();

            var financialTransactionDto1 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "test",
            };
            var financialTransactionPostResponse1 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto1);
            financialTransactionPostResponse1.EnsureSuccessStatusCode();
            var financialTransactionDto3 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 300,
                Comment = "test",
            };
            var financialTransactionPostResponse3 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto3);
            financialTransactionPostResponse3.EnsureSuccessStatusCode();
            // Act
            var response = await _client.GetAsync("/api/transactions?page=1&size=2&sortBy=amount");
            response.EnsureSuccessStatusCode();

            // Assert
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BaseFinancialTransactionOutputDto>>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(100, result.Data[0].Amount);
            Assert.Equal(200, result.Data[1].Amount);
        }
        [Fact]
        public async Task GetReport_FinancialTransactionsWithSeveralCategories_ReturnsTwoGroups()
        {
            // Arrange
            await _fixture.ResetCheckpointAsync();
            var categoryDto1 = new CategoryInputDto { Name = "abc" };

            var categoryPostResponse1 = await _client.PostAsJsonAsync("/api/categories", categoryDto1);
            categoryPostResponse1.EnsureSuccessStatusCode();

            var categoryDto2 = new CategoryInputDto { Name = "def" };

            var categoryPostResponse2 = await _client.PostAsJsonAsync("/api/categories", categoryDto2);
            categoryPostResponse2.EnsureSuccessStatusCode();

            var financialTransactionDto2 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 200,
                Comment = "test",
            };
            var financialTransactionPostResponse2 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto2);
            financialTransactionPostResponse2.EnsureSuccessStatusCode();

            var financialTransactionDto1 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "test",
            };
            var financialTransactionPostResponse1 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto1);
            financialTransactionPostResponse1.EnsureSuccessStatusCode();
            var financialTransactionDto3 = new BaseFinancialTransactionInputDto
            {
                CategoryId = 2,
                Amount = 300,
                Comment = "test",
            };
            var financialTransactionPostResponse3 = await _client.PostAsJsonAsync("/api/transactions", financialTransactionDto3);
            financialTransactionPostResponse3.EnsureSuccessStatusCode();
            // Act
            var response = await _client.GetAsync("/api/transactions/report");
            response.EnsureSuccessStatusCode();

            // Assert
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<GroupedReportDto>>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("abc", result.Data[0].Key.Category);
            Assert.Equal(2, result.Data[0].Transactions.Count);
            Assert.Equal("def", result.Data[1].Key.Category);
            Assert.Single(result.Data[1].Transactions);
        }
        [Fact]
        public async Task CreateAndGetById_ValidCategory_ReturnsCreatedCategory()
        {
            await _fixture.ResetCheckpointAsync();
            var firstCategory = new CategoryInputDto { Name = "abc" };
            var categoryPostResponse = await _client.PostAsJsonAsync("/api/categories", firstCategory);
            categoryPostResponse.EnsureSuccessStatusCode();
            var transactionDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "test",
            };
            var postResponse2 = await _client.PostAsJsonAsync("/api/transactions", transactionDto);
            postResponse2.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/transactions/1");
            getResponse.EnsureSuccessStatusCode();

            var result = await getResponse.Content.ReadFromJsonAsync<ApiResponse<BaseFinancialTransactionOutputDto>>();
            Assert.NotNull(result);
            Assert.Equal(100, result.Data.Amount);
            Assert.Equal("test", result.Data.Comment);
        }
        [Fact]
        public async Task Update_ExistingFinancialTransaction_ReturnsUpdatedFinancialTransaction()
        {
            await _fixture.ResetCheckpointAsync();

            var firstCategory = new CategoryInputDto { Name = "abc" };
            var categoryPostResponse = await _client.PostAsJsonAsync("/api/categories", firstCategory);
            categoryPostResponse.EnsureSuccessStatusCode();

            var transactionDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "Original",
            };

            var financialTransactionResponse = await _client.PostAsJsonAsync("/api/transactions", transactionDto);
            financialTransactionResponse.EnsureSuccessStatusCode();

            var updateDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "Updated",
            };

            var updateResponse = await _client.PutAsJsonAsync("/api/transactions/1", updateDto);
            updateResponse.EnsureSuccessStatusCode();

            var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<BaseFinancialTransactionOutputDto>>();
            Assert.Equal("Updated", updated.Data.Comment);
        }
        [Fact]
        public async Task Delete_ExistingFinancialTransaction_ReturnsNotFoundAfterDeletedFinancialTransaction()
        {
            await _fixture.ResetCheckpointAsync();

            var firstCategory = new CategoryInputDto { Name = "abc" };
            var categoryPostResponse = await _client.PostAsJsonAsync("/api/categories", firstCategory);
            categoryPostResponse.EnsureSuccessStatusCode();
            var transactionDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "test",
            };
            var financialTransactionPostResponse = await _client.PostAsJsonAsync("/api/transactions", transactionDto);
            financialTransactionPostResponse.EnsureSuccessStatusCode();

            var getBeforeDeleteResponse = await _client.GetAsync("/api/transactions/1");
            getBeforeDeleteResponse.EnsureSuccessStatusCode();

            var deleteResponse = await _client.DeleteAsync("/api/transactions/1");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/transactions/1");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
        [Fact]
        public async Task Create_InvalidNoRequiredParametersFinancialTransaction_ReturnsValidationError()
        {
            await _fixture.ResetCheckpointAsync();

            var invalidFinancialTransactions = new BaseFinancialTransactionInputDto { };
            var response = await _client.PostAsJsonAsync("/api/transactions", invalidFinancialTransactions);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal("VALIDATION_ERROR", apiResponse.Error.Code);
            Assert.Equal("Validation failed", apiResponse.Error.Message);
        }
    }
}
