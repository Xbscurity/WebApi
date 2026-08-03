using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Services.FinancialTransactions;
using api.Specifications;
using api.Tests.Unit.Factories;
using Ardalis.Specification;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class FinancialTransactionServiceTests
    {
        private readonly Mock<ICurrentUser> _currentUserMock = new();
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock = new();
        private readonly Mock<IFinancialTransactionRepository> _financialTransactionRepositoryMock = new();
        private readonly Mock<IGroupingReportStrategy> _strategyMock = new();
        private readonly FinancialTransactionService _sut;

        private const string CurrentUserId = "current-user";
        private const string OtherUserId = "other-user";
        public FinancialTransactionServiceTests()
        {
            _strategyMock
                .Setup(s => s.Key)
                .Returns(GroupingReportStrategyKey.ByCategory);

            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(CurrentUserId);

            _sut = new FinancialTransactionService(
                Mock.Of<ILogger<FinancialTransactionService>>(),
                _currentUserMock.Object,
                _categoryRepositoryMock.Object,
                _financialTransactionRepositoryMock.Object,
                [_strategyMock.Object]);
        }

        [Fact]
        public async Task GetAllAsync_InvalidSortBy_ReturnsValidationError()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "InvalidSortBy"
            };

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                "FT_INVALID_SORT_BY",
                result.FirstError.Code);

            _financialTransactionRepositoryMock.Verify(
                x => x.ListAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()),
                Times.Never);

            _financialTransactionRepositoryMock.Verify(
                x => x.CountAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()),
                Times.Never);
        }

        [Theory]
        [InlineData("category")]
        [InlineData("amount")]
        [InlineData("createdat")]
        public async Task GetAllAsync_ValidSortBy_ReturnsPagedItems(string sortBy)
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = sortBy
            };


            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var transactions = new List<FinancialTransactionOutputDto>
                {
                    new ()
    {
                    Id = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "food",
                    Amount = 123,
                    Type = FinancialTransactionType.Income,
                    Comment = "123",
                    CreatedAt = timeStub,
                    UpdatedAt = timeStub
                },
                    new ()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "food",
                    Amount = 123,
                    Type = FinancialTransactionType.Income,
                    Comment = "123",
                    CreatedAt = timeStub,
                    UpdatedAt = timeStub
                }
                };

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            _financialTransactionRepositoryMock
                .Setup(x => x.CountAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(25);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal(25, result.Value.Pagination.TotalItems);
            Assert.Equal(1, result.Value.Pagination.PageNumber);
            Assert.Equal(10, result.Value.Pagination.PageSize);
            Assert.Equal(transactions, result.Value.Items);
        }

        [Theory]
        [InlineData("Category")]
        [InlineData("AMOUNT")]
        [InlineData("CREATEDAT")]
        public async Task GetAllAsync_SortByDifferentCase_IsTreatedAsValid(string sortBy)
        {
            // Arrange
            var query = new EntityQuery
            {
                SortBy = sortBy,
            };

            var transactions = new List<FinancialTransactionOutputDto>();

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
        }

        [Fact]
        public async Task GetAllAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "createdat"
            };

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var transactions = new List<FinancialTransactionOutputDto>();

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<FinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            _financialTransactionRepositoryMock
                .Setup(x => x.CountAsync(It.IsAny<FinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(0);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Empty(result.Value.Items);
            Assert.Equal(query.Page, result.Value.Pagination.PageNumber);
            Assert.Equal(query.Size, result.Value.Pagination.PageSize);
            Assert.Equal(0, result.Value.Pagination.TotalItems);
            Assert.False(result.Value.Pagination.HasNext);
            Assert.False(result.Value.Pagination.HasPrevious);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _sut.GetByIdAsync(financialTransactionId);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.FT.NotFound(financialTransactionId), result.FirstError);
        }

        [Fact]
        public async Task GetByIdAsync_CorrectInput_ReturnsDto()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var outputDto = new FinancialTransactionOutputDto
            {
                Id = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                CategoryName = "food",
                Amount = 123,
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
            };

            _financialTransactionRepositoryMock
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<FinancialTransactionByIdWithCategorySpecification>()))
                .ReturnsAsync(outputDto);

            // Act
            var result = await _sut.GetByIdAsync(financialTransactionId);

            //Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(outputDto, result.Value);
        }

        [Fact]
        public async Task CreateAsync_CategoryNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var input = new FinancialTransactionCreateInputDto
            {
                CategoryId = Guid.NewGuid(),
                Amount = 123,
                Comment = "123",
                Type = FinancialTransactionType.Income,
            };

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(input.CategoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(input.CategoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<FinancialTransaction>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CategoryBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId);
            var dto = new FinancialTransactionCreateInputDto
            {
                CategoryId = category.Id,
                Amount = 123,
                Comment = "123",
                Type = FinancialTransactionType.Income

            };

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<FinancialTransaction>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ValidId_ReturnsCreatedDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId);
            var input = new FinancialTransactionCreateInputDto
            {
                CategoryId = category.Id,
                Comment = "test",
                Amount = 10,
                Type = FinancialTransactionType.Income,
            };

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            FinancialTransaction? addedTransaction = null;

            _financialTransactionRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<FinancialTransaction, CancellationToken>((t, ct) => addedTransaction = t)
                .ReturnsAsync((FinancialTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.NotNull(addedTransaction);
            Assert.Equal(input.CategoryId, addedTransaction!.CategoryId);
            Assert.Equal(input.Type, addedTransaction.Type);
            Assert.Equal(input.Amount, addedTransaction.Amount);
            Assert.Equal(input.Comment, addedTransaction.Comment);
            Assert.Equal(CurrentUserId, addedTransaction.AppUserId);

            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(input.CategoryId, result.Value.CategoryId);
            Assert.Equal(input.Type, result.Value.Type);
            Assert.Equal(input.Amount, result.Value.Amount);
            Assert.Equal(input.Comment, result.Value.Comment);
            Assert.Equal(category.Name, result.Value.CategoryName);

            _financialTransactionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_FinancialTransactionNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                CategoryId = Guid.NewGuid(),
                Comment = "123",
                Type = FinancialTransactionType.Income
            };

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.FT.NotFound(financialTransactionId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_FinancialTransactionBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();
            var financialTransaction = FinancialTransactionFactory.Create(
                id: financialTransactionId, userId: OtherUserId);

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                CategoryId = Guid.NewGuid(),
                Comment = "123",
                Type = FinancialTransactionType.Income
            };

            _financialTransactionRepositoryMock.Setup(
                x => x.GetByIdAsync(financialTransactionId)).
                ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.FT.NotFound(financialTransactionId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_CategoryNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();
            var financialTransaction = FinancialTransactionFactory.Create(
                id: financialTransactionId, userId: CurrentUserId);

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                CategoryId = Guid.NewGuid(),
                Comment = "123",
                Type = FinancialTransactionType.Income
            };

            _financialTransactionRepositoryMock.Setup(
                x => x.GetByIdAsync(financialTransaction.Id)).
                ReturnsAsync(financialTransaction);

            _categoryRepositoryMock.Setup(
                x => x.GetByIdAsync(input.CategoryId)).
                ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.Category.NotFound(input.CategoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_CategoryBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();
            var financialTransaction = FinancialTransactionFactory.Create(
                amount: 100,
                id: financialTransactionId,
                userId: CurrentUserId);
            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                CategoryId = Guid.NewGuid(),
                Comment = "test",
                Type = FinancialTransactionType.Income
            };

            var category = CategoryFactory.Create(id: input.CategoryId, userId: OtherUserId);

            _financialTransactionRepositoryMock.Setup(
                x => x.GetByIdAsync(financialTransactionId)).
                ReturnsAsync(financialTransaction);

            _categoryRepositoryMock.Setup(
                x => x.GetByIdAsync(input.CategoryId)).
                ReturnsAsync(category);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.Category.NotFound(input.CategoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsUpdatedDto()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();


            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                CategoryId = Guid.NewGuid(),
                Comment = "new",
                Type = FinancialTransactionType.Expense
            };
            var financialTransaction = FinancialTransactionFactory.Create(
                id: financialTransactionId, userId: CurrentUserId);
            var category = CategoryFactory.Create(id: input.CategoryId, userId: CurrentUserId);

            _financialTransactionRepositoryMock.Setup(
                x => x.GetByIdAsync(financialTransactionId)).
                ReturnsAsync(financialTransaction);

            _categoryRepositoryMock.Setup(
                x => x.GetByIdAsync(input.CategoryId)).
                ReturnsAsync(category);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(input.CategoryId, financialTransaction.CategoryId);
            Assert.Equal(input.Amount, financialTransaction.Amount);
            Assert.Equal(input.Comment, financialTransaction.Comment);
            Assert.Equal(input.Type, financialTransaction.Type);

            Assert.Equal(input.CategoryId, result.Value.CategoryId);
            Assert.Equal(input.Amount, result.Value.Amount);
            Assert.Equal(input.Comment, result.Value.Comment);
            Assert.Equal(input.Type, result.Value.Type);
            Assert.Equal(category.Name, result.Value.CategoryName);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _sut.DeleteAsync(financialTransactionId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.FT.NotFound(financialTransactionId), result.FirstError);

            _financialTransactionRepositoryMock
                .Verify(
                x => x.DeleteAsync(It.IsAny<FinancialTransaction>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_FinancialTransactionBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var financialTransaction = FinancialTransactionFactory
                .Create(id: financialTransactionId, userId: OtherUserId);

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.DeleteAsync(financialTransactionId);

            // Arrange
            Assert.True(result.IsError);
            Assert.Equal(Errors.FT.NotFound(financialTransactionId), result.FirstError);

            _financialTransactionRepositoryMock
                .Verify(
                x => x.DeleteAsync(It.IsAny<FinancialTransaction>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ValidFinancialTransaction_DeletesSuccessfully()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var financialTransaction = FinancialTransactionFactory
                .Create(id: financialTransactionId, userId: CurrentUserId);

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.DeleteAsync(financialTransactionId);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(Result.Deleted, result.Value);

            _financialTransactionRepositoryMock
                .Verify(
                x => x.DeleteAsync(financialTransaction),
                Times.Once);
        }

        [Fact]
        public async Task GetReportAsync_UnsupportedStrategy_ReturnsValidationError()
        {
            // Arrange
            var query = new ReportQuery
            {
                Key = (GroupingReportStrategyKey)999,
                Page = 1,
                Size = 10
            };

            // Act
            var result = await _sut.GetReportAsync(query);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.FT.UnsupportedStrategy(query.Key), result.FirstError);
        }

        [Fact]
        public async Task GetReportAsync_ValidStrategy_ReturnsPagedItems()
        {
            // Arrange
            var query = new ReportQuery
            {
                Key = GroupingReportStrategyKey.ByCategory,
                Page = 1,
                Size = 10
            };

            var expectedItems = new List<GroupedReportOutputDto>
        {
            new()
            {
                GroupKey = new ReportKey.CategoryKey("Food"),
                Count = 2,
                TotalAmount = 150.00m,
                Transactions = []
            },
            new()
            {
                GroupKey = new ReportKey.CategoryKey("Transport"),
                Count = 3,
                TotalAmount = 300.00m,
                Transactions = []
            }
        };
            const int totalCount = 5;

            _strategyMock
                .Setup(s => s.GetGroupedAsync(It.IsAny<Specification<FinancialTransaction>>(), query))
                .ReturnsAsync(expectedItems);

            _financialTransactionRepositoryMock
                .Setup(r => r.CountAsync(It.IsAny<Specification<FinancialTransaction>>()))
                .ReturnsAsync(totalCount);

            // Act
            var result = await _sut.GetReportAsync(query);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(expectedItems, result.Value.Items);
            Assert.Equal(query.Page, result.Value.Pagination.PageNumber);
            Assert.Equal(query.Size, result.Value.Pagination.PageSize);
            Assert.Equal(totalCount, result.Value.Pagination.TotalItems);
        }
    }
}
