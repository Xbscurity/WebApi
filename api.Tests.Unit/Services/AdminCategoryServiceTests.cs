using api.Dtos.Category;
using api.Interfaces;
using api.Models;
using api.Queries;
using api.Services.Categories;
using api.Services.User;
using api.Specifications;
using api.Tests.Unit.Factories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class AdminCategoryServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock = new();
        private readonly Mock<IRepository<FinancialTransaction>> _financialTransactionRepositoryMock = new();
        private readonly AdminCategoryService _sut;
        private const string OtherUserId = "other-user";
        public AdminCategoryServiceTests()
        {
            _sut = new AdminCategoryService(
                Mock.Of<ILogger<CategoryService>>(),
                _userServiceMock.Object,
                _categoryRepositoryMock.Object,
                _financialTransactionRepositoryMock.Object
                );
        }

        [Fact]
        public async Task GetAllAsync_InvalidSortBy_ReturnsValidationError()
        {
            // Arange 
            var query = new AdminEntityQuery
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
                x => x.ListAsync(It.IsAny<AdminCategorySortedPagedSpecification>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                x => x.CountAsync(It.IsAny<AdminCategorySortedPagedSpecification>()),
                Times.Never);
        }

        [Theory]
        [InlineData("name")]
        [InlineData("isactive")]
        [InlineData("createdat")]
        public async Task GetAllAsync_CorrectQuery_ReturnsPaginatedItems(string sortBy)
        {
            // Arrange

            var query = new AdminEntityQuery
            {
                SortBy = sortBy,
                Page = 1,
                Size = 10,
            };


            var timeStub = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var categories = new List<AdminCategoryOutputDto>
            {
                new()
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
                AppUserId = "123"
            },
                new()
            {
                Id = Guid.NewGuid(),
                Name = "Transport",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
                AppUserId = "123"
            }
            };


            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<AdminCategorySortedPagedSpecification>()))
                .ReturnsAsync(categories);

            _categoryRepositoryMock
                .Setup(x => x.CountAsync(
                    It.IsAny<AdminCategorySortedPagedSpecification>()))
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
        [InlineData("NAME")]
        [InlineData("ISACTIVE")]
        [InlineData("CREATEDAT")]
        public async Task GetAllAsync_SortByDifferentCase_IsTreatedAsValid(string sortBy)
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                SortBy = sortBy,
            };

            var categories = new List<AdminCategoryOutputDto>();

            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<AdminCategorySortedPagedSpecification>()))
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

            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "isactive"
            };

            var categories = new List<AdminCategoryOutputDto>();

            _categoryRepositoryMock
                .Setup(x => x.ListAsync(
                    It.IsAny<AdminCategorySortedPagedSpecification>()))
                .ReturnsAsync(categories);

            _categoryRepositoryMock
                .Setup(x => x.CountAsync(
                    It.IsAny<AdminCategorySortedPagedSpecification>()))
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

            var category = new AdminCategoryOutputDto
            {
                Id = categoryId,
                Name = "Transport",
                IsActive = true,
                CreatedAt = timeStub,
                UpdatedAt = timeStub,
                AppUserId = OtherUserId
            };

            _categoryRepositoryMock
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<AdminCategoryByIdSpecification>()))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.GetByIdAsync(categoryId);

            //Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(category, result.Value);
        }

        [Fact]
        public async Task CreateAsync_ValidTargetUser_ReturnsCreatedCategoryForTargetUser()
        {
            // Arrange     
            var input = new AdminCategoryCreateInputDto
            {
                Name = "Test",
                AppUserId = OtherUserId
            };

            _userServiceMock
                .Setup(x => x.AnyAsync(OtherUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal(input.AppUserId, result.Value.AppUserId);
            Assert.Equal(input.Name, result.Value.Name);

            _userServiceMock.Verify(
                x => x.AnyAsync(OtherUserId),
                Times.Once);

            _categoryRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Category>(c =>
                c.Name == "Test" &&
                c.AppUserId == OtherUserId)),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NameWithWhitespace_TrimsName()
        {
            // Arrange
            var input = new AdminCategoryCreateInputDto
            {
                Name = "  Test  ",
                AppUserId = OtherUserId
            };

            _userServiceMock
                .Setup(x => x.AnyAsync(OtherUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CreateAsync(input);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal("Test", result.Value.Name);

            _categoryRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Category>(c => c.Name == "Test" &&
                c.AppUserId == OtherUserId)),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var targetUserId = "bad-user";

            var dto = new AdminCategoryCreateInputDto
            {
                Name = "Test",
                AppUserId = targetUserId
            };

            _userServiceMock
                .Setup(x => x.AnyAsync(targetUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.User.NotFound(targetUserId), result.FirstError);

            _userServiceMock.Verify(x => x.AnyAsync(targetUserId),
                Times.Once);

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var dto = new CategoryUpdateInputDto
            {
                Name = "Updated"
            };

            _categoryRepositoryMock
               .Setup(x => x.GetByIdAsync(categoryId))
               .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.UpdateAsync(categoryId, dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(Errors.Category.NotFound(categoryId), result.FirstError);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidInput_UpdatesCategoryAndReturnsDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId, name: "old");

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var dto = new CategoryUpdateInputDto
            {
                Name = "New Name"
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, dto);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

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

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId, name: "old");

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var dto = new CategoryUpdateInputDto
            {
                Name = "     New Name    "
            };

            // Act
            var result = await _sut.UpdateAsync(categoryId, dto);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");
            Assert.Equal("New Name", category.Name);
            Assert.Equal("New Name", result.Value.Name);

            _categoryRepositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SetActiveAsync_NonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.SetActiveAsync(categoryId, true);

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
        public async Task SetActiveAsync_ValidInput_UpdatesActiveStatusAndReturnsDto(bool isActive)
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId, isActive: true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.SetActiveAsync(categoryId, isActive);

            // Assert
            Assert.True(result.IsSuccess, $"Error code: {result.FirstError.Code}");

            Assert.Equal(isActive, result.Value.ToggleActive);

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

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_HasFinancialTransactions_ReturnsRestrictedError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId);

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

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ValidCategory_DeletesSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = CategoryFactory.Create(id: categoryId, userId: OtherUserId);

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

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(category),
                Times.Once);
        }
    }
}
