using api.Controllers;
using api.Dtos.Category;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Moq;
namespace api.Tests.Controllers
{

    public class CategoryControllerTests
    {
        private readonly Mock<ICategoriesRepository> _categoriesRepositoryMock;
        private readonly CategoriesController _controller;
        public CategoryControllerTests()
        {
            _categoriesRepositoryMock = new Mock<ICategoriesRepository>();
            _controller = new CategoriesController(_categoriesRepositoryMock.Object);
        }
        [Theory]
        [MemberData(nameof(GetAllCategoriesTestData))]
        public async Task GetAll_WithCategoriesOrEmpty_ReturnsExpectedCount(List<Category> categories, int expectedCount)
        {
            // Arrange
            _categoriesRepositoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var response = Assert.IsType<ApiResponse<List<Category>>>(result);
            Assert.Equal(expectedCount, response.Data.Count);
            Assert.Null(response.Error);
        }
        public static IEnumerable<object[]> GetAllCategoriesTestData()
        {
            yield return new object[] { new List<Category> { new Category { Id = 1, Name = "Category 1" }, new Category { Id = 2, Name = "Category 2" } }, 2 };
            yield return new object[] { new List<Category>(), 0 };
        }
        [Fact]
        public async Task GetById_CategoryExists_ReturnsOkWithCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category 1" };
            const int existingCategoryId = 1;
            _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(existingCategoryId)).ReturnsAsync(category);

            // Act
            var result = await _controller.GetById(1);

            // Assert

            var response = Assert.IsType<ApiResponse<Category>>(result);
            Assert.Equal(category.Name, response.Data.Name);
            Assert.Null(response.Error);
        }

        [Fact]
        public async Task GetById_CategoryNotFound_ReturnsNotFound()
        {
            // Arrange
             int categoryNotFoundId = 999;
            _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(categoryNotFoundId)).ReturnsAsync((Category?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            var response = Assert.IsType<ApiResponse<Category>>(result);
            Assert.Equal("Category not found", response.Error.Message);
            Assert.Equal("NOT_FOUND", response.Error.Code);
        }
        [Fact]
        public async Task Create_ValidCategoryDto_ReturnsOk()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };

            var createdCategory = new Category { Id = 1, Name = categoryDto.Name };

            _categoriesRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _controller.Create(categoryDto);

            // Assert
            Assert.IsType<ApiResponse<Category>>(response);
            Assert.Equal(categoryDto.Name, response.Data.Name);
            Assert.Null(response.Error);
            _categoriesRepositoryMock.Verify(r => r.CreateAsync(It.Is<Category>(c => c.Name == categoryDto.Name)), Times.Once);
        }
        [Fact]
        public async Task Update_CategoryExists_ReturnsOkWithUpdatedCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };
            var existingCategoryId = 1;
            var receivedCategory = new Category {Id = existingCategoryId};
            var updatedCategory = new Category
            {
                Id = existingCategoryId,
                Name = categoryDto.Name
            };
            _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(existingCategoryId)).ReturnsAsync(receivedCategory);
            _categoriesRepositoryMock.Setup(r => r.UpdateAsync(receivedCategory)).ReturnsAsync(updatedCategory);

            // Act
            var response = await _controller.Update(existingCategoryId, categoryDto);

            //Assert
            var result = Assert.IsType<ApiResponse<Category>>(response);
            Assert.Equal(existingCategoryId, result.Data.Id);
            Assert.Equal(categoryDto.Name, result.Data.Name);
            Assert.Null(response.Error);
        }
        [Fact]
        public async Task Update_CategoryNotExists_ReturnsNotFound()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };
            var notExistingCategoryId = 999;
            _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(notExistingCategoryId)).ReturnsAsync((Category?)null);

            // Act
            var response = await _controller.Update(notExistingCategoryId, categoryDto);

            //Assert
            var result = Assert.IsType<ApiResponse<Category>>(response);
            Assert.Equal("Category not found", response.Error.Message);
            Assert.Equal("NOT_FOUND", response.Error.Code);
        }
        [Fact]
        public async Task Delete_CategoryExists_ReturnsOkwithTrue()
        {
            //Arrange
            var existingCategoryId = 1;
            _categoriesRepositoryMock.Setup(r => r.DeleteAsync(existingCategoryId)).ReturnsAsync(true);

            //Act
            var response = await _controller.Delete(existingCategoryId);

            //Assert
            var result = Assert.IsType<ApiResponse<bool>>(response);
            Assert.True(result.Data);
            Assert.Null(response.Error);
        }
        [Fact]
        public async Task Delete_CategoryNotExists_ReturnsOkwithTrue()
        {
            //Arrange
            var NotExistingCategoryId = 999;
            _categoriesRepositoryMock.Setup(r => r.DeleteAsync(NotExistingCategoryId)).ReturnsAsync(false);

            //Act
            var response = await _controller.Delete(NotExistingCategoryId);

            //Assert
            var result = Assert.IsType<ApiResponse<bool>>(response);
            Assert.Equal("Category not found", response.Error.Message);
            Assert.Equal("NOT_FOUND", response.Error.Code);
        }
    }
}

