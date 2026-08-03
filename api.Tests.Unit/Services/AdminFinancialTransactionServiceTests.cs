using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Interfaces;
using api.Models;
using api.Queries;
using api.Services.FinancialTransactions;
using api.Specifications;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class AdminFinancialTransactionServiceTests
    {
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock = new();
        private readonly Mock<IFinancialTransactionRepository> _financialTransactionRepositoryMock = new();
        private const string OtherUserId = "other-user";
        private readonly AdminFinancialTransactionService _sut;
        public AdminFinancialTransactionServiceTests()
        {
            _sut = new AdminFinancialTransactionService(
                Mock.Of<ILogger<AdminFinancialTransactionService>>(),
                _categoryRepositoryMock.Object,
                _financialTransactionRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_InvalidSortBy_ReturnsValidationError()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "invalidField"
            };

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                "FT_INVALID_SORT_BY",
                result.FirstError.Code);

            _financialTransactionRepositoryMock.Verify(
                x => x.ListAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()),
                Times.Never);

            _financialTransactionRepositoryMock.Verify(
                x => x.CountAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()),
                Times.Never);
        }

        [Theory]
        [InlineData("category")]
        [InlineData("amount")]
        [InlineData("createdAt")]
        public async Task GetAllAsync_ValidSortBy_ReturnsPagedItems(string sortBy)
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = sortBy
            };

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var transactions = new List<AdminFinancialTransactionOutputDto>
            {
                new()
            {
                Id = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                CategoryName = "food",
                Amount = 123,
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
                AppUserId = "123"
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
                UpdatedAt = timeStub,
                AppUserId = "123"
            }
            };

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            _financialTransactionRepositoryMock
                .Setup(x => x.CountAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()))
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
        [InlineData("CATEGORY")]
        [InlineData("AMOUNT")]
        [InlineData("CREATEDAT")]
        public async Task GetAllAsync_SortByDifferentCase_IsTreatedAsValid(string sortBy)
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                SortBy = sortBy
            };

            var transactions = new List<AdminFinancialTransactionOutputDto>();

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()))
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
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
            };

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var transactions = new List<AdminFinancialTransactionOutputDto>();

            _financialTransactionRepositoryMock
                .Setup(x => x.ListAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            _financialTransactionRepositoryMock
                .Setup(x => x.CountAsync(It.IsAny<AdminFinancialTransactionSortedPagedSpecification>()))
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

            var outputDto = new AdminFinancialTransactionOutputDto
            {
                Id = financialTransactionId,
                CategoryId = Guid.NewGuid(),
                CategoryName = "food",
                Amount = 123,
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
                AppUserId = "123"
            };

            _financialTransactionRepositoryMock
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<AdminFinancialTransactionByIdSpecification>()))
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
            var input = new AdminFinancialTransactionCreateInputDto
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
        public async Task CreateAsync_CorrectInput_CreatesFinancialTransactionAndReturnsDto()
        {
            // Arrange
            var input = new AdminFinancialTransactionCreateInputDto
            {
                CategoryId = Guid.NewGuid(),
                Amount = 123,
                Comment = "123",
                Type = FinancialTransactionType.Income,
            };

            var category = CategoryFactory.Create(id: input.CategoryId, userId: OtherUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(input.CategoryId))
                .ReturnsAsync(category);

            FinancialTransaction? addedTransaction = null;

            _financialTransactionRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<FinancialTransaction, CancellationToken>((t, ct) => addedTransaction = t)
                .ReturnsAsync((FinancialTransaction t, CancellationToken ct) => t);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.NotNull(addedTransaction);
            Assert.Equal(input.CategoryId, addedTransaction!.CategoryId);
            Assert.Equal(input.Amount, addedTransaction.Amount);
            Assert.Equal(input.Comment, addedTransaction.Comment);
            Assert.Equal(input.Type, addedTransaction.Type);
            Assert.Equal(category.AppUserId, addedTransaction.AppUserId);

            Assert.Equal(input.CategoryId, result.Value.CategoryId);
            Assert.Equal(input.Amount, result.Value.Amount);
            Assert.Equal(input.Comment, result.Value.Comment);
            Assert.Equal(input.Type, result.Value.Type);
            Assert.Equal(category.AppUserId, result.Value.AppUserId);
            Assert.Equal(category.Name, result.Value.CategoryName);

            _financialTransactionRepositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()),
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
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CategoryId = Guid.NewGuid(),
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
        public async Task UpdateAsync_CategoryNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();
            var financialTransaction = FinancialTransactionFactory
                .Create(id: financialTransactionId, userId: OtherUserId);

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CategoryId = Guid.NewGuid(),
            };

            _financialTransactionRepositoryMock
               .Setup(x => x.GetByIdAsync(financialTransactionId))
               .ReturnsAsync(financialTransaction);

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
        public async Task UpdateAsync_CategoryBelongsToDifferentUser_ReturnsForbiddenError()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var financialTransaction = FinancialTransactionFactory
                .Create(id: financialTransactionId, userId: OtherUserId);

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 10,
                Type = FinancialTransactionType.Income,
                Comment = "123",
                CategoryId = Guid.NewGuid(),
            };

            var category = CategoryFactory.Create(userId: "any-other-user", id: input.CategoryId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(input.CategoryId))
                .ReturnsAsync(category);

            _financialTransactionRepositoryMock
               .Setup(x => x.GetByIdAsync(financialTransactionId))
               .ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(
                Errors.FT.UserMismatch(
                    input.CategoryId,
                    category.AppUserId,
                    financialTransaction.AppUserId),
                result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidInput_UpdatesAndReturnsDto()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 100,
                Type = FinancialTransactionType.Expense,
                Comment = "new",
                CategoryId = Guid.NewGuid(),
            };

            var category = CategoryFactory.Create(id: input.CategoryId, userId: OtherUserId);
            var financialTransaction = FinancialTransactionFactory.Create(
                id: financialTransactionId, userId: OtherUserId, categoryId: input.CategoryId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(input.CategoryId))
                .ReturnsAsync(category);

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(
                result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(input.Amount, result.Value.Amount);
            Assert.Equal(input.Comment, result.Value.Comment);
            Assert.Equal(input.Type, result.Value.Type);
            Assert.Equal(input.CategoryId, result.Value.CategoryId);

            Assert.Equal(input.Amount, financialTransaction.Amount);
            Assert.Equal(input.Comment, financialTransaction.Comment);
            Assert.Equal(input.Type, financialTransaction.Type);
            Assert.Equal(input.CategoryId, financialTransaction.CategoryId);

            _financialTransactionRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CommentWithWhitespace_TrimsComment()
        {
            // Arrange
            var financialTransactionId = Guid.NewGuid();

            var input = new FinancialTransactionUpdateInputDto
            {
                Amount = 100,
                Type = FinancialTransactionType.Expense,
                Comment = "   new   ",
                CategoryId = Guid.NewGuid(),
            };

            var category = CategoryFactory.Create(id: input.CategoryId, userId: OtherUserId);
            var financialTransaction = FinancialTransactionFactory.Create(
                id: financialTransactionId, userId: OtherUserId, categoryId: input.CategoryId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(input.CategoryId))
                .ReturnsAsync(category);

            _financialTransactionRepositoryMock
                .Setup(x => x.GetByIdAsync(financialTransactionId))
                .ReturnsAsync(financialTransaction);

            // Act
            var result = await _sut.UpdateAsync(financialTransactionId, input);

            // Assert
            Assert.True(
                result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal("new", result.Value.Comment);

            Assert.Equal("new", financialTransaction.Comment);


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

            _financialTransactionRepositoryMock.Verify(
                x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()),
                Times.Never);

            _financialTransactionRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<FinancialTransaction>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_DeletesSuccessfully()
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

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(Result.Deleted, result.Value);

            _financialTransactionRepositoryMock.Verify(
                x => x.DeleteAsync(financialTransaction),
                Times.Once);
        }
    }
}