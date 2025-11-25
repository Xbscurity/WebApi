using api.Dtos.FinancialTransaction;
using api.Models;
using api.Providers.Interfaces;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Repositories.Interfaces;
using api.Services.Transaction;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;

namespace api.Tests.Unit.Services.FinancialTranasction
{
    public class FinancialTransactionServiceTests
    {
        private readonly Mock<IFinancialTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ITimeProvider> _timeStub;
        private readonly FinancialTransactionService _transactionService;
        public FinancialTransactionServiceTests()
        {

            _transactionRepositoryMock = new Mock<IFinancialTransactionRepository>();

            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            _timeStub = new Mock<ITimeProvider>();
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeStub.Setup(t => t.UtcNow).Returns(fixedTime);

            var strategiesStub = new List<IGroupingReportStrategy> { };

            var loggerStub = Mock.Of<ILogger<FinancialTransactionService>>();

            _transactionService = new FinancialTransactionService(
                _transactionRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                loggerStub,
                _timeStub.Object,
                strategiesStub);
        }
        [Fact]
        public async Task CreateForAdminAsync_ValidInput_ReturnsCorrectDto()
        {
            // Arrange
            var inputDto = new AdminFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                AppUserId = "user-123",
                Comment = "Test",
            };

            _transactionRepositoryMock.Setup(r => r.CreateAsync(
                It.Is<FinancialTransaction>(t =>
                    t.CategoryId == inputDto.CategoryId &&
                    t.Amount == inputDto.Amount &&
                    t.AppUserId == inputDto.AppUserId &&
                    t.Comment == inputDto.Comment)))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.CreateForAdminAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equivalent(inputDto, result);
        }
        [Fact]
        public async Task CreateForUserAsync_ValidInput_ReturnsCorrectDto()
        {
            // Arrange
            var userId = "user-id";
            var inputDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "Test",
            };

            _transactionRepositoryMock.Setup(r => r.CreateAsync(
                It.Is<FinancialTransaction>(t =>
                    t.CategoryId == inputDto.CategoryId &&
                    t.Amount == inputDto.Amount &&
                    t.AppUserId == userId &&
                    t.Comment == inputDto.Comment)))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.CreateForUserAsync(userId, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equivalent(inputDto, result);
        }

        [Fact]
        public async Task DeleteAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            var receivedFinancialTranasction = new FinancialTransaction(_timeStub.Object)
            {
                Id = 1,
            };

            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(receivedFinancialTranasction.Id))
                .ReturnsAsync(receivedFinancialTranasction);

            // Act
            var result = await _transactionService.DeleteAsync(receivedFinancialTranasction.Id);

            // Assert
            Assert.True(result);

            _transactionRepositoryMock.Verify(
                r => r.GetByIdAsync(receivedFinancialTranasction.Id),
                Times.Once);

            _transactionRepositoryMock.Verify(
                r => r.DeleteAsync(receivedFinancialTranasction),
                Times.Once);
        }

        [Theory]
        [InlineData(4, null)]
        [InlineData(2, "user123")]
        public async Task GetAllForAdminAsync_GivenUserId_ReturnsExpectedTransactions(int expectedCount, string userId)
        {
            // Arrange
            var queryObject = new PaginationQueryObject
            {
                Page = 1,
                Size = 10,
            };

            var financialTransactionsMock = new List<FinancialTransaction>
            {
                new (_timeStub.Object) {Id = 1, AppUserId = "user123", Category = new Category {Id = 1 } },
                new (_timeStub.Object) {Id = 2, AppUserId = "user123", Category = new Category {Id = 1 } },
                new (_timeStub.Object) {Id = 3, AppUserId = "otherUserId", Category = new Category {Id = 1 } },
                new (_timeStub.Object) {Id = 4, AppUserId = "otherUserId", Category = new Category {Id = 1 } },
            }.AsQueryable().BuildMockDbSet();

            _transactionRepositoryMock.Setup(r => r.GetQueryableWithCategory()).Returns(financialTransactionsMock.Object);

            // Act
            var result = await _transactionService.GetAllForAdminAsync(queryObject, userId);

            // Assert
            Assert.Equal(expectedCount, result.Data.Count);
            var expectedFinancialTransactionIds = Enumerable.Range(1, expectedCount);
            Assert.Equal(expectedFinancialTransactionIds, result.Data.Select(c => c.Id));

            Assert.Equal(expectedCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetAllForAdminAsync_EmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var queryObject = new PaginationQueryObject
            {
                Page = 1,
                Size = 10,
            };

            var financialTransactionsMock = new List<FinancialTransaction>().AsQueryable().BuildMockDbSet();

            _transactionRepositoryMock.Setup(r => r.GetQueryableWithCategory()).Returns(financialTransactionsMock.Object);

            // Act
            var result = await _transactionService.GetAllForAdminAsync(queryObject, It.IsAny<string>());

            // Assert
            Assert.Empty(result.Data);

            Assert.Equal(0, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetAllForUserAsync_ValidInput_ReturnsCorrectPagedData()
        {
            // Arrange
            var queryObject = new PaginationQueryObject
            {
                Page = 1,
                Size = 10,
            };
            var userId = "user123";
            var financialTransactionsMock = new List<FinancialTransaction>
            {
                new (_timeStub.Object) {Id = 1, AppUserId = userId, Category = new Category {Id = 1 } },
                new (_timeStub.Object) {Id = 2, AppUserId = userId, Category = new Category {Id = 1 } },
                new (_timeStub.Object) {Id = 3, AppUserId = "otherUserId", Category = new Category {Id = 1 } },
            }.AsQueryable().BuildMockDbSet();

            _transactionRepositoryMock.Setup(r => r.GetQueryableWithCategory()).Returns(financialTransactionsMock.Object);

            // Act
            var result = await _transactionService.GetAllForUserAsync(userId, queryObject);

            // Assert
            var expectedCount = financialTransactionsMock.Object.Count(x => x.AppUserId == userId);
            Assert.Equal(expectedCount, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);

            Assert.Equal(expectedCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetAllForUserAsync_EmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var queryObject = new PaginationQueryObject
            {
                Page = 1,
                Size = 10,
            };

            var userId = "user123";

            var financialTransactionsMock = new List<FinancialTransaction>().AsQueryable().BuildMockDbSet();

            _transactionRepositoryMock.Setup(r => r.GetQueryableWithCategory()).Returns(financialTransactionsMock.Object);

            // Act
            var result = await _transactionService.GetAllForUserAsync(userId, queryObject);

            // Assert
            Assert.Empty(result.Data);

            Assert.Equal(0, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetById_ValidInput_ReturnsCorrectDto()
        {
            // Arrange
            var returnedFinancialTransaction = new FinancialTransaction(_timeStub.Object)
            {
                Id = 1,
                Amount = 100,
                AppUserId = "user-123",
                CategoryId = 1,
                Category = new Category
                {
                    Id = 1,
                    Name = "Test",
                }
            };

            _transactionRepositoryMock.Setup(
                r => r.GetByIdAsync(returnedFinancialTransaction.Id)).
                ReturnsAsync(returnedFinancialTransaction);

            // Act
            var result = await _transactionService.GetByIdAsync(returnedFinancialTransaction.Id);

            // Assert
            var expectedDto = new BaseFinancialTransactionOutputDto
            {
                Id = returnedFinancialTransaction.Id,
                Amount = returnedFinancialTransaction.Amount,
                AppUserId = returnedFinancialTransaction.AppUserId,
                CategoryId = returnedFinancialTransaction.CategoryId,
                CategoryName = returnedFinancialTransaction.Category.Name,
                Comment = returnedFinancialTransaction.Comment,
                CreatedAt = returnedFinancialTransaction.CreatedAt,
            };
            Assert.Equal(expectedDto, result);

            _transactionRepositoryMock.Verify(
                r => r.GetByIdAsync(returnedFinancialTransaction.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsCorrectDto()
        {
            // Arrange
            var returnedFinancialTransaction = new FinancialTransaction(_timeStub.Object)
            {
                Id = 1,
                Amount = 100,
                AppUserId = "user-123",
                CategoryId = 1,
                Category = new Category
                {
                    Id = 1,
                    Name = "Test",
                }
            };
            var inputDto = new BaseFinancialTransactionInputDto()
            {
                Amount = 100,
                CategoryId = 2,
                Comment = "Updated"
            };

            _transactionRepositoryMock.Setup(
                r => r.GetByIdAsync(returnedFinancialTransaction.Id)).
                ReturnsAsync(returnedFinancialTransaction);

            // Act
            var result = await _transactionService.UpdateAsync(returnedFinancialTransaction.Id, inputDto);

            // Assert
            var expectedDto = new BaseFinancialTransactionOutputDto
            {
                Id = returnedFinancialTransaction.Id,
                Amount = inputDto.Amount,
                AppUserId = returnedFinancialTransaction.AppUserId,
                CategoryId = inputDto.CategoryId,
                CategoryName = returnedFinancialTransaction.Category.Name,
                Comment = returnedFinancialTransaction.Comment,
                CreatedAt = returnedFinancialTransaction.CreatedAt,
            };
            Assert.NotNull(result);

            _transactionRepositoryMock.Verify(
                r => r.GetByIdAsync(returnedFinancialTransaction.Id), Times.Once);

            _transactionRepositoryMock.Verify(
                r => r.UpdateAsync(returnedFinancialTransaction), Times.Once);
        }       
    }
}
