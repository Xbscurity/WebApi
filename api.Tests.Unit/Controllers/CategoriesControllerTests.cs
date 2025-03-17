using api.Controllers;
using api.Dtos.Category;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Moq;
namespace api.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoriesRepository> _repositoryMock;
        private readonly CategoriesController _controller;
        public CategoriesControllerTests()
        {
            _repositoryMock = new Mock<ICategoriesRepository>();
            _controller = new CategoriesController(_repositoryMock.Object);
        }
        [Theory]
        [MemberData(nameof(GetAllCategoriesTestDataAndCount))]
        public async Task GetAll_ListOfTransactions_ReturnsExpectedCount(List<Category> categories, int expectedCount)
        {
            // Arrange
            _repositoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.Equal(expectedCount, result.Data.Count);
            Assert.Null(result.Error);
        }
        public static IEnumerable<object[]> GetAllCategoriesTestDataAndCount()
        {
            yield return new object[]
            {
                new List<Category>
                {
                    new Category(),
                    new Category()
                },
                2
            };
            yield return new object[] { new List<Category>(), 0 };
        }
        [Fact]
        public async Task GetById_CategoryExists_ReturnsOkWithCategory()
        {
            // Arrange
            const int existingCategoryId = 1;
            var category = new Category { Id = existingCategoryId};
            _repositoryMock.Setup(r => r.GetByIdAsync(existingCategoryId)).ReturnsAsync(category);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.Equal(category.Id, result.Data.Id);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task GetById_CategoryNotExists_ReturnsNotFound()
        {
            // Arrange
            const int categoryNotFoundId = 999;
            _repositoryMock.Setup(r => r.GetByIdAsync(categoryNotFoundId)).ReturnsAsync((Category?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.Equal("Category not found", result.Error.Message);
            Assert.Equal("NOT_FOUND", result.Error.Code);
        }
        [Fact]
        public async Task Update_CategoryExists_ReturnsOkWithUpdatedCategory1()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };
            const int existingCategoryId = 1;
            var receivedCategory = new Category { Id = existingCategoryId, Name = "Old Category" };
            var categoryToUpdate = new Category
            {
                Id = existingCategoryId,
                Name = categoryDto.Name
            };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingCategoryId)).ReturnsAsync(receivedCategory);
            _repositoryMock.Setup(r => r.UpdateAsync(categoryToUpdate)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(existingCategoryId, categoryDto);

            //Assert
            Assert.Equal(existingCategoryId, result.Data.Id);
            Assert.Equal(categoryDto.Name, result.Data.Name);
            Assert.Null(result.Error);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Category>(c => c.Name == categoryDto.Name)), Times.Once);
        }
        [Fact]
        public async Task Update_CategoryNotExists_ReturnsNotFound()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };
            const int notExistingCategoryId = 999;
            _repositoryMock.Setup(r => r.GetByIdAsync(notExistingCategoryId)).ReturnsAsync((Category?)null);

            // Act
            var result = await _controller.Update(notExistingCategoryId, categoryDto);

            //Assert
            Assert.Equal("Category not found", result.Error.Message);
            Assert.Equal("NOT_FOUND", result.Error.Code);
        }
        [Fact]
        public async Task Delete_CategoryExists_ReturnsOkwithTrue()
        {
            //Arrange
            const int existingCategoryId = 1;
            var receivedCategory = new Category { Id = existingCategoryId };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingCategoryId)).ReturnsAsync(receivedCategory);
            _repositoryMock.Setup(r => r.DeleteAsync(receivedCategory)).Returns(Task.CompletedTask);

            //Act
            var result = await _controller.Delete(existingCategoryId);

            //Assert
            Assert.True(result.Data);
            Assert.Null(result.Error);
            _repositoryMock.Verify(r => r.DeleteAsync(receivedCategory), Times.Once);
        }
        [Fact]
        public async Task Delete_CategoryNotExists_ReturnsNotFound()
        {
            //Arrange
            const int NotExistingCategoryId = 999;

            //Act
            var result = await _controller.Delete(NotExistingCategoryId);

            //Assert
            Assert.Equal("Category not found", result.Error.Message);
            Assert.Equal("NOT_FOUND", result.Error.Code);
        }
    }
}

