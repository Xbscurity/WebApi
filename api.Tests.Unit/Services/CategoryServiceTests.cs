using api.Dtos.Category;
using api.Extensions;
using api.Migrations;
using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Services.Categories;
using api.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;

        private readonly string TestUserId = "user123";
        private readonly string OtherUserId = "user456";
        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            var loggerStub = Mock.Of<ILogger<CategoryService>>();
            _categoryService = new CategoryService(_categoryRepositoryMock.Object, loggerStub);
        }

        [Fact]
        public async Task GetAllForUserAsync_ExcludeInactiveCategories_ReturnsOnlyOwnActiveCategories()
        {
            // Arrange
            var categories = TestData.GetCategories(TestUserId, OtherUserId);

            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(TestUserId, queryObject, includeInactive: false);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllForUserAsync_IncludeInactiveCategories_ReturnsAllOwnCategories()
        {
            // Arrange
            var categories = TestData.GetCategories(TestUserId, OtherUserId);

            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(TestUserId, queryObject, includeInactive: true);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllForUserAsync_NoCategories_ReturnsEmptyList()
        {
            // Arrange
            var categories = new List<Category>().AsQueryable();

            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(TestUserId, queryObject, includeInactive: true);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.NotNull(result.Pagination);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_WithoutUserId_ReturnsAllCategories()
        {
            // Arrange
            var categories = TestData.GetCategories(TestUserId, OtherUserId);
            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: null);

            // Assert
            Assert.Equal(8, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 8);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_WithUserId_ReturnsOnlyThatUserCategories()
        {
            // Arrange
            var categories = TestData.GetCategories(TestUserId, OtherUserId);
            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: TestUserId);

            // Assert
            Assert.Equal(4, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal(TestUserId, c.AppUserId));

            // Verify
            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_NoCategories_ReturnsEmptyList()
        {
            // Arrange
            var emptyCategories = new List<Category>().AsQueryable();
            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(emptyCategories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.NotNull(result.Pagination);

            // Verify
            _categoryRepositoryMock.Verify(r => r.GetQueryable(), Times.Once);
        }

        [Fact]
        public async Task ToggleActiveAsync_NonExistingCategory_ReturnsFalse()
        {
            // Arrange
            var nonExistingCategoryId = 999;
            _categoryRepositoryMock.
                Setup(r => r.GetByIdAsync(nonExistingCategoryId)).
                ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.ToggleActiveAsync(nonExistingCategoryId);

            // Assert
            Assert.False(result);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(nonExistingCategoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ToggleActiveAsync_ExistingCategory_ReturnsTrue(bool isActive)
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
                IsActive = isActive
            };
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            // Act
            var result = await _categoryService.ToggleActiveAsync(existingCategory.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(!isActive, existingCategory.IsActive);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(existingCategory.Id), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingCategory_ReturnsNull()
        {
            // Arrange
            const int nonExistingCategoryId = 999;
            var categoryDto = new BaseCategoryUpdateInputDto { Name = "Anything" };
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingCategoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.UpdateAsync(nonExistingCategoryId, categoryDto);

            // Assert
            Assert.Null(result);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(nonExistingCategoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);

        }

        [Fact]
        public async Task UpdateAsync_ExistingCategory_ReturnsUpdatedCategory()
        {
            // Arrange
            var receivedCategory = new Category
            {
                Id = 1,
                Name = "Old"
            };
            var categoryDto = new BaseCategoryUpdateInputDto { Name = "Updated" };

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(receivedCategory.Id))
                .ReturnsAsync(receivedCategory);

            // Act
            var result = await _categoryService.UpdateAsync(receivedCategory.Id, categoryDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
            Assert.Equal(receivedCategory.Id, result.Id);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(receivedCategory.Id),Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Category>(
                c => c.Id == receivedCategory.Id && c.Name == "Updated")),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingCategory_ReturnsFalse()
        {
            // Arrange
            const int nonExistingCategoryId = 999;
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingCategoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.DeleteAsync(nonExistingCategoryId);

            // Assert
            Assert.False(result);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(nonExistingCategoryId), Times.Once);

            _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ExistingCategory_ReturnsTrue()
        {
            // Arrange
            var retrievedCategory = new Category
            {
                Id = 1,
            };
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(retrievedCategory.Id))
                .ReturnsAsync(retrievedCategory);

            _categoryRepositoryMock
                .Setup(r => r.DeleteAsync(retrievedCategory))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.DeleteAsync(retrievedCategory.Id);

            // Assert
            Assert.True(result);

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(retrievedCategory.Id), Times.Once);

            _categoryRepositoryMock.Verify(r => r.DeleteAsync(retrievedCategory), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingCategory_ReturnsNull()
        {
            // Arrange
            var nonExistingCategoryId = 999;

            _categoryRepositoryMock.
                Setup(r => r.GetByIdAsync(nonExistingCategoryId)).
                ReturnsAsync((Category?)null);
            // Act
            var result = await _categoryService.GetByIdAsync(nonExistingCategoryId);

            // Assert
            Assert.Null(result);

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(nonExistingCategoryId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingCategory_ReturnsRetrievedCategory()
        {
            // Arrange
            var retrievedCategory = new Category
            {
                Id = 1,
                Name = "Test",
            };

            _categoryRepositoryMock.
                Setup(r => r.GetByIdAsync(retrievedCategory.Id)).
                ReturnsAsync(retrievedCategory);
            // Act
            var result = await _categoryService.GetByIdAsync(retrievedCategory.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(retrievedCategory.Id, result!.Id);
            Assert.Equal(retrievedCategory.Name, result.Name);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(retrievedCategory.Id), Times.Once);
        }
        [Fact]
        public async Task GetByIdRawAsync_NonExistingCategory_ReturnsNull()
        {
            // Arrange
            var nonExistingCategoryId = 999;

            _categoryRepositoryMock.
                Setup(r => r.GetByIdAsync(nonExistingCategoryId)).
                ReturnsAsync((Category?)null);
            // Act
            var result = await _categoryService.GetByIdRawAsync(nonExistingCategoryId);

            // Assert
            Assert.Null(result);

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(nonExistingCategoryId), Times.Once);
        }

        [Fact]
        public async Task GetByIdRawAsync_ExistingCategory_ReturnsRetrievedCategory()
        {
            // Arrange
            var retrievedCategory = new Category
            {
                Id = 1,
                Name = "Test",
            };

            _categoryRepositoryMock.
                Setup(r => r.GetByIdAsync(retrievedCategory.Id)).
                ReturnsAsync(retrievedCategory);
            // Act
            var result = await _categoryService.GetByIdRawAsync(retrievedCategory.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(retrievedCategory, result);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(retrievedCategory.Id), Times.Once);
        }
    }
}
