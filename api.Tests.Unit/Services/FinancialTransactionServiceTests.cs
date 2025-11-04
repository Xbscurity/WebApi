using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using api.Providers.Interfaces;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Repositories.Interfaces;
using api.Services.Categories;
using api.Services.Transaction;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Serilog.Core;

namespace api.Tests.Unit.Services
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
        public async Task UpdateAsync_NonExistingCategory_ReturnsNull()
        {
            // Arrange
            const int nonExistingCategoryId = 999;
            var inputDto = new BaseFinancialTransactionInputDto
            {
                CategoryId = nonExistingCategoryId,
                Comment = "Anything",
                Amount = 100
            };
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingCategoryId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _transactionService.UpdateAsync(nonExistingCategoryId, inputDto);

            // Assert
            Assert.Null(result);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync());
            _transactionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FinancialTransaction>()), Times.Never);

        }

        [Fact]
        public async Task UpdateAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingTransactionId = 1;
            var receivedTransaction = new FinancialTransaction(_timeStub.Object)
            { Id = existingTransactionId, Comment = "Test" };
            var inputDto = new BaseFinancialTransactionInputDto { Comment = "Updated" };

            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(existingTransactionId))
                .ReturnsAsync(receivedTransaction);

            _transactionRepositoryMock
                .Setup(r => r.UpdateAsync(
                    It.Is<FinancialTransaction>(t => t.Id == existingTransactionId && t.Comment == "Updated")))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _transactionService.UpdateAsync(existingTransactionId, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Comment);
            Assert.Equal(existingTransactionId, result.Id);
            _transactionRepositoryMock.Verify();
        }

        [Fact]
        public async Task DeleteAsync_CategoryNotExists_ReturnsFalse()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(notExistingFinancialTransactionId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _transactionService.DeleteAsync(notExistingFinancialTransactionId);

            // Assert
            Assert.False(result);
            _transactionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<FinancialTransaction>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            var receivedCategory = new FinancialTransaction(_timeStub.Object)
            { Id = existingFinancialTransactionId, Comment = "Test" };
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(existingFinancialTransactionId))
                .ReturnsAsync(receivedCategory);

            _transactionRepositoryMock
                .Setup(r => r.DeleteAsync(receivedCategory))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _transactionService.DeleteAsync(existingFinancialTransactionId);

            // Assert
            Assert.True(result);
            _transactionRepositoryMock.Verify();
        }
        [Fact]
        public async Task GetReportAsync_ByCategoryKey_ReturnsCorrectGroupedData()
        {
            // Arrange
            var queryObject = new ReportQueryObject
            {
                Key = GroupingReportStrategyKey.ByCategory
            };
            var testData = new List<FinancialTransaction>
{
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 1,
        Category = new Category { Name = "Food" }
    },
    new FinancialTransaction(_timeStub.Object)
    {
         Id = 2,
        Category = new Category { Name = "Food" }
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 3,
        Category = new Category { Name = "Tech" }
    }

};
            var mockDbSet = testData.AsQueryable().BuildMockDbSet();
            var mockRepo = new Mock<IFinancialTransactionRepository>();
            mockRepo.Setup(r => r.GetQueryableWithCategory()).Returns(mockDbSet.Object);
            var strategy = new GroupByCategoryStrategy();

            var strategies = new List<IGroupingReportStrategy>
{
    strategy
};
            var newTransactionService = new TransactionService(mockRepo.Object,
                _timeStub.Object, strategies);
            // Act

            var result = await newTransactionService.GetReportAsync(queryObject);

            // Assert
            Assert.Equal(2, result.Data.Count);

            var foodGroup = result.Data.FirstOrDefault(g => g.Key.Category == "Food");
            Assert.NotNull(foodGroup);
            Assert.Equal(2, foodGroup.Transactions.Count);
            Assert.Contains(foodGroup.Transactions, t => t.Id == 1);
            Assert.Contains(foodGroup.Transactions, t => t.Id == 2);

            var techGroup = result.Data.FirstOrDefault(g => g.Key.Category == "Tech");
            Assert.NotNull(techGroup);
            Assert.Single(techGroup.Transactions);
            Assert.Equal(3, techGroup.Transactions[0].Id);

        }
        [Fact]
        public async Task GetReportAsync_ByDateKey_ReturnsCorrectGroupedData()
        {
            // Arrange
            var queryObject = new ReportQueryObject
            {
                Key = GroupingReportStrategyKey.ByDate
            };
            var testData = new List<FinancialTransaction>
{
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 1,
        CreatedAt = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero)
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 2,
        CreatedAt = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero)
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 3,
        CreatedAt = new DateTimeOffset(2023, 2, 10, 0, 0, 0, TimeSpan.Zero)
    }

};
            var mockDbSet = testData.AsQueryable().BuildMockDbSet();
            var mockRepo = new Mock<IFinancialTransactionRepository>();
            mockRepo.Setup(r => r.GetQueryableWithCategory()).Returns(mockDbSet.Object);
            var strategy = new GroupByDateStrategy();

            var strategies = new List<IGroupingReportStrategy>
{
    strategy
};
            var newTransactionService = new TransactionService(mockRepo.Object,
                _timeStub.Object, strategies);
            // Act

            var result = await newTransactionService.GetReportAsync(queryObject);

            // Assert
            Assert.Equal(2, result.Data.Count);

            var januaryGroup = result.Data.FirstOrDefault(g => g.Key.Month == 1);
            Assert.NotNull(januaryGroup);
            Assert.Equal(2, januaryGroup.Transactions.Count);
            Assert.Contains(januaryGroup.Transactions, t => t.Id == 1);
            Assert.Contains(januaryGroup.Transactions, t => t.Id == 2);

            var februaryGroup = result.Data.FirstOrDefault(g => g.Key.Month == 2);
            Assert.NotNull(februaryGroup);
            Assert.Single(februaryGroup.Transactions);
            Assert.Equal(3, februaryGroup.Transactions[0].Id);


        }
        [Fact]
        public async Task GetReportAsync_ByCategoryAndDateKey_ReturnsCorrectGroupedData()
        {
            // Arrange
            var queryObject = new ReportQueryObject
            {
                Key = GroupingReportStrategyKey.ByCategoryAndDate
            };
            var testData = new List<FinancialTransaction>
{
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 1,
        Category = new Category{ Name = "Food"},
        CreatedAt = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero)
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 2,
        Category = new Category{ Name = "Food"},
        CreatedAt = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero)
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 3,
        Category = new Category{ Name = "Food"},
        CreatedAt = new DateTimeOffset(2023, 2, 10, 0, 0, 0, TimeSpan.Zero)
    },
    new FinancialTransaction(_timeStub.Object)
    {
        Id = 4,
        Category = new Category{ Name = "Tech"},
        CreatedAt = new DateTimeOffset(2023, 2, 10, 0, 0, 0, TimeSpan.Zero)
    }
};
            var mockDbSet = testData.AsQueryable().BuildMockDbSet();
            var mockRepo = new Mock<IFinancialTransactionRepository>();
            mockRepo.Setup(r => r.GetQueryableWithCategory()).Returns(mockDbSet.Object);
            var strategy = new GroupByDateAndCategoryStrategy();
            var strategies = new List<IGroupingReportStrategy> { strategy };
            var newTransactionService = new TransactionService(mockRepo.Object,
                _timeStub.Object, strategies);

            // Act

            var result = await newTransactionService.GetReportAsync(queryObject);

            // Assert
            Assert.Equal(3, result.Data.Count);

            var januaryFoodGroup = result.Data.FirstOrDefault(g => g.Key.Month == 1 && g.Key.Category == "Food");
            Assert.NotNull(januaryFoodGroup);
            Assert.Equal(2, januaryFoodGroup.Transactions.Count);
            Assert.Contains(januaryFoodGroup.Transactions, t => t.Id == 1);
            Assert.Contains(januaryFoodGroup.Transactions, t => t.Id == 2);

            var februaryFoodGroup = result.Data.FirstOrDefault(g => g.Key.Month == 2 && g.Key.Category == "Food");
            Assert.NotNull(februaryFoodGroup);
            Assert.Single(februaryFoodGroup.Transactions);
            Assert.Equal(3, februaryFoodGroup.Transactions[0].Id);

            var februaryTechGroup = result.Data.FirstOrDefault(g => g.Key.Month == 2 && g.Key.Category == "Tech");
            Assert.NotNull(februaryTechGroup);
            Assert.Single(februaryTechGroup.Transactions);
            Assert.Equal(4, februaryTechGroup.Transactions[0].Id);
        }
    }
}
