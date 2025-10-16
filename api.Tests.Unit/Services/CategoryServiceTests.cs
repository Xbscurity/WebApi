using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Services.Categories;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;
        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            var loggerStub = Mock.Of<ILogger<CategoryService>>();
            _categoryService = new CategoryService(_categoryRepositoryMock.Object, loggerStub);
        }
        [Fact]
        public async Task GetAllForUserAsync_ExcludeInactiveCategories_ReturnsOnlyActiveUserAndGlobalCategories()
        {
            // Arrange
            var userId = "user1";
            var categories = new List<Category>
        {
            new() { Id = 1, Name = "User Active", AppUserId = userId, IsActive = true },
            new() { Id = 2, Name = "User Inactive", AppUserId = userId, IsActive = false },
            new() { Id = 3, Name = "Global Active", AppUserId = null, IsActive = true },
            new() { Id = 4, Name = "Global Inactive", AppUserId = null, IsActive = false },
        }.AsQueryable();

            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(userId, queryObject, includeInactive: false);

            // Assert
            var names = result.Data.Select(d => d.Name).ToList();

            Assert.Equal(2, names.Count);
            Assert.Contains("User Active", names);
            Assert.Contains("Global Active", names);
        }

        [Fact]
        public async Task GetAllForUserAsync_IncludeInactiveCategories_ReturnsAllUserAndGlobalActiveCategories()
        {
            // Arrange
            var userId = "user1";
            var categories = new List<Category>
        {
            new() { Id = 1, Name = "User Active", AppUserId = userId, IsActive = true },
            new() { Id = 2, Name = "User Inactive", AppUserId = userId, IsActive = false },
            new() { Id = 3, Name = "Global Active", AppUserId = null, IsActive = true },
        }.AsQueryable();

            _categoryRepositoryMock.Setup(r => r.GetQueryable()).Returns(categories);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(userId, queryObject, includeInactive: true);

            // Assert
            var names = result.Data.Select(d => d.Name).ToList();

            Assert.Equal(3, names.Count);
            Assert.Contains("User Active", names);
            Assert.Contains("User Inactive", names);
            Assert.Contains("Global Active", names);
        }

        [Fact]
        public async Task UpdateAsync_CategoryNotExists_ReturnsNull()
        {
            // Arrange
            const int notExistingCategoryId = 999;
            var categoryDto = new CategoryInputDto { Name = "Anything" };
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(notExistingCategoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.UpdateAsync(notExistingCategoryId, categoryDto);

            // Assert
            Assert.Null(result);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);

        }

        [Fact]
        public async Task UpdateAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingCategoryId = 1;
            var receivedCategory = new Category { Id = existingCategoryId, Name = "Test" };
            var categoryDto = new CategoryInputDto { Name = "Updated" };

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(existingCategoryId))
                .ReturnsAsync(receivedCategory);

            _categoryRepositoryMock
                .Setup(r => r.UpdateAsync(It.Is<Category>(c => c.Id == existingCategoryId && c.Name == "Updated")))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _categoryService.UpdateAsync(existingCategoryId, categoryDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Name);
            Assert.Equal(existingCategoryId, result.Id);
            _categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task DeleteAsync_CategoryNotExists_ReturnsFalse()
        {
            // Arrange
            const int notExistingCategoryId = 999;
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(notExistingCategoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.DeleteAsync(notExistingCategoryId);

            // Assert
            Assert.False(result);
            _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingCategoryId = 1;
            var receivedCategory = new Category { Id = existingCategoryId, Name = "Test" };
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(existingCategoryId))
                .ReturnsAsync(receivedCategory);

            _categoryRepositoryMock
                .Setup(r => r.DeleteAsync(receivedCategory))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _categoryService.DeleteAsync(existingCategoryId);

            // Assert
            Assert.True(result);
            _categoryRepositoryMock.Verify();
        }

    }
}
