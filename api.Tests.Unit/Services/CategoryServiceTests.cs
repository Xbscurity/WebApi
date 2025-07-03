using api.Models;
using api.Repositories.Interfaces;
using api.Services;
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
            _categoryService = new CategoryService(_categoryRepositoryMock.Object);
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
