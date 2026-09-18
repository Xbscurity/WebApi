using api.Data;
using api.Dtos.Category;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Repositories;
using api.Services.Categories;
using api.Specifications.Categories;
using api.Specifications.FinancialTransactions;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICurrentUser> _currentUserMock = new();
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock = new();
        private readonly Mock<IRepository<FinancialTransaction>> _financialTransactionRepositoryMock = new();
        private readonly CategoryService _sut;

        private const string CurrentUserId = "current-user";
        private const string OtherUserId = "other-user";

        public CategoryServiceTests()
        {
            _currentUserMock
                .SetupGet(x => x.UserId)
                .Returns(CurrentUserId);

            _sut = new CategoryService(
                Mock.Of<ILogger<CategoryService>>(),
                _currentUserMock.Object,
                _categoryRepositoryMock.Object,
                _financialTransactionRepositoryMock.Object
                );
        }

        [Fact]
        public async Task GetAllAsync_InvalidSortBy_ReturnsValidationError()
        {
            // Arrange 

            var query = new EntityQuery
            {
                SortBy = "InvalidSortBy"
            };

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsError);

            var error = result.FirstError;

            Assert.Equal("CATEGORY_INVALID_SORT_BY", error.Code);

            _categoryRepositoryMock.Verify(
                x => x.ListAsync(It.IsAny<CategorySortedPagedSpecification>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                x => x.CountAsync(It.IsAny<CategorySortedPagedSpecification>()),
                Times.Never);
        }

        [Theory]
        [InlineData("name")]
        [InlineData("isactive")]
        [InlineData("createdat")]
        public async Task GetAllAsync_CorrectQuery_ReturnsPaginatedItems(string sortBy)
        {
            // Arrange

            var query = new EntityQuery
            {
                SortBy = sortBy,
                Page = 1,
                Size = 10,
            };

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var categories = new List<CategoryOutputDto>
            {
                new ()
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub
            },
                new ()
            {
                Id = Guid.NewGuid(),
                Name = "Transport",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub
            },
            };

            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<CategorySortedPagedSpecification>()))
                .ReturnsAsync(categories);

            _categoryRepositoryMock
                .Setup(x => x.CountAsync(
                    It.IsAny<CategorySortedPagedSpecification>()))
                .ReturnsAsync(25);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(2, result.Value.Items.Count);
            Assert.Equal(1, result.Value.Pagination.PageNumber);
            Assert.Equal(10, result.Value.Pagination.PageSize);
            Assert.Equal(25, result.Value.Pagination.TotalItems);
            Assert.True(result.Value.Pagination.HasNext);
            Assert.False(result.Value.Pagination.HasPrevious);
            Assert.Equal(categories, result.Value.Items);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("ISACTIVE")]
        [InlineData("CREATEDAT")]
        public async Task GetAllAsync_SortByDifferentCase_IsTreatedAsValid(string sortBy)
        {
            // Arrange

            var query = new EntityQuery
            {
                SortBy = sortBy,
            };

            var categories = new List<CategoryOutputDto>();

            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<CategorySortedPagedSpecification>()))
                .ReturnsAsync(categories);

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
                SortBy = "isactive"
            };

            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var transactions = new List<CategoryOutputDto>();

            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<CategorySortedPagedSpecification>()))
                .ReturnsAsync(transactions);

            _categoryRepositoryMock
                .Setup(x => x.CountAsync(It.IsAny<CategorySortedPagedSpecification>()))
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
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.GetByIdAsync(categoryId);

            // Assert
            Assert.True(result.IsError);

            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);
        }

        [Fact]
        public async Task GetByIdAsync_CorrectInput_ReturnsCategoryDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var category = new CategoryOutputDto
            {
                Id = categoryId,
                Name = "Transport",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub
            };

            _categoryRepositoryMock
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<CategoryByIdSpecification>()))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.GetByIdAsync(categoryId);

            //Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(category, result.Value);
        }

        [Fact]
        public async Task CreateAsync_Valid_ReturnsCreatedCategoryForCurrentUser()
        {
            // Arrange
            var input = new CategoryCreateInputDto
            {
                Name = "New",
            };

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(input.Name, result.Value.Name);

            _categoryRepositoryMock.Verify(
                x =>
                x.AddAsync(It.Is<Category>(c => c.AppUserId == CurrentUserId && c.Name == "New")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NameAlreadyExists_ReturnsNameAlreadyExistsError()
        {
            // Arrange
            var input = new CategoryCreateInputDto
            {
                Name = "New",
            };
            _categoryRepositoryMock
                .Setup(c => c.AnyAsync(
                    It.IsAny<HasCategoryWithNameSpecification>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NameAlreadyExists(input.Name), result.FirstError);

            _categoryRepositoryMock.Verify(
                x =>
                x.AddAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_NameWithWhitespace_TrimsName()
        {
            // Arrange
            var input = new CategoryCreateInputDto
            {
                Name = "  Test  ",
            };

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal("Test", result.Value.Name);

            _categoryRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Category>(c => c.Name == "Test")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var input = new CategoryUpdateInputDto
            {
                Name = "Updated"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.UpdateAsync(categoryId, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_CategoryBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId, name: "old");

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var input = new CategoryUpdateInputDto
            {
                Name = "New Name"
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);
            Assert.Equal("old", category.Name);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidInput_UpdatesCategoryAndReturnsDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var input = new CategoryUpdateInputDto
            {
                Name = "  New Name  "
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(categoryId, result.Value.Id);
            Assert.Equal("New Name", result.Value.Name);
            Assert.Equal("New Name", category.Name);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NameWithWhitespace_TrimsName()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var input = new CategoryUpdateInputDto
            {
                Name = "    Test       ",
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal("Test", result.Value.Name);
            Assert.Equal("Test", category.Name);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NameAlreadyExists_ReturnsNameAlreadyExistsError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId, name: "Old Name");

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.AnyAsync(It.IsAny<HasCategoryWithNameSpecification>()))
                .ReturnsAsync(true);

            var input = new CategoryUpdateInputDto
            {
                Name = "Existing Name"
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NameAlreadyExists(input.Name), result.FirstError);
            Assert.Equal("Old Name", category.Name);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SetActiveAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            var input = new SetActiveInputDto
            {
                IsActive = true
            };

            // Act
            var result = await _sut.SetActiveAsync(categoryId, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SetActiveAsync_CategoryBelongsToDifferentUser_ReturnsNotFoundError(bool isActive)
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId, isActive: isActive);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var input = new SetActiveInputDto
            {
                IsActive = isActive
            };

            // Act
            var result = await _sut.SetActiveAsync(categoryId, input);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);
            Assert.Equal(isActive, category.IsActive);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SetActiveAsync_ValidInput_UpdatesActiveStatusAndReturnsDto(bool isActive)
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId, isActive: true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var input = new SetActiveInputDto
            {
                IsActive = isActive
            };

            // Act
            var result = await _sut.SetActiveAsync(categoryId, input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(isActive, result.Value.IsActive);
            Assert.Equal(isActive, category.IsActive);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.DeleteAsync(categoryId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CategoryBelongsToDifferentUser_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.DeleteAsync(categoryId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_HasFinancialTransactions_ReturnsRestrictedError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _financialTransactionRepositoryMock
                .Setup(x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteAsync(categoryId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.DeleteRestricted(categoryId), result.FirstError);

            _financialTransactionRepositoryMock.Verify(
                x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()),
                Times.Once);

            _categoryRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ValidCategory_DeletesSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: CurrentUserId);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _financialTransactionRepositoryMock
                .Setup(x => x.AnyAsync(It.IsAny<HasFinancialTransactionsByCategoryIdSpecification>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteAsync(categoryId);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(Result.Deleted, result.Value);

            _categoryRepositoryMock.Verify(
                x => x.DeleteAsync(category),
                Times.Once);
        }

        [Fact]
        public async Task CreateInitialCategoriesForUserAsync_ValidUserId_AddsDefaultCategoriesForUser()
        {
            // Act
            await _sut.CreateInitialCategoriesForUserAsync(CurrentUserId);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddRangeAsync(It.Is<List<Category>>(list =>
                    list.Count == DataSeeder.DefaultCategoryTemplates.Count &&
                    list.All(c => c.AppUserId == CurrentUserId && c.IsActive))),
                Times.Once);
        }
    }
}
