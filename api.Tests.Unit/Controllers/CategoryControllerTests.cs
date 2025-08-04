using api.Controllers;
using api.Models;
using api.QueryObjects;
using api.Services.Categories;
using Moq;
namespace api.Tests.Unit.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly UserCategoryController _controller;
        public CategoriesControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();
            _controller = new UserCategoryController(_serviceMock.Object);
        }
        [Fact]
        public async Task GetAll_InvalidSortByField_ReturnsBadRequest()
        {
            // Arrange
            var paginationQueryObject = new PaginationQueryObject
            {
                SortBy = "InvalidField"
            };

            // Act
            var result = await _controller.GetAll(paginationQueryObject);

            // Assert
            Assert.Equal("BAD_REQUEST", result.Error?.Code);
        }
        [Theory]
        [InlineData("Id")]
        [InlineData("Name")]
        public async Task GetAll_ValidSortByField_ReturnsOk(string sortBy)
        {
            // Arrange
            var paginationQueryObject = new PaginationQueryObject
            {
                SortBy = sortBy
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(paginationQueryObject))
                .ReturnsAsync(new PagedData<Category>());

            // Act
            var result = await _controller.GetAll(paginationQueryObject);

            // Assert
            Assert.Null(result.Error);
        }
        [Fact]
        public async Task GetById_CategoryExists_ReturnsOkWithCategory()
        {
            // Arrange
            const int existingCategoryId = 1;
            var category = new Category { Id = existingCategoryId };
            _serviceMock
                .Setup(s => s.GetByIdAsync(existingCategoryId))
                .ReturnsAsync(category);

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
            _serviceMock
                .Setup(s => s.GetByIdAsync(categoryNotFoundId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.Equal("Category not found", result.Error?.Message);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
        }
        [Fact]
        public async Task Update_CategoryExists_ReturnsOkWithUpdatedCategory1()
        {
            // Arrange
            const int existingCategoryId = 1;
            var categoryDto = new CategoryInputDto() { Name = "Test" };
            var updatedCategory = new Category() { Id = 1, Name = "Test" };
            _serviceMock
                .Setup(s => s.UpdateAsync(existingCategoryId, categoryDto))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _controller.Update(existingCategoryId, categoryDto);

            // Assert
            Assert.Null(result.Error);
            Assert.Equal("Test", result.Data.Name);
        }
        [Fact]
        public async Task Update_CategoryNotExists_ReturnsNotFound()
        {
            // Arrange
            var categoryDto = new CategoryInputDto { Name = "New Category" };
            const int notExistingCategoryId = 999;
            _serviceMock
                .Setup(s => s.UpdateAsync(notExistingCategoryId, categoryDto))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _controller.Update(notExistingCategoryId, categoryDto);

            // Assert
            Assert.Equal("Category not found", result.Error?.Message);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
        }
        [Fact]
        public async Task Delete_CategoryExists_ReturnsOkwithTrue()
        {
            // Arrange
            const int existingCategoryId = 1;
            _serviceMock
                .Setup(s => s.DeleteAsync(existingCategoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(existingCategoryId);

            // Assert
            Assert.True(result.Data);
            Assert.Null(result.Error);
        }
        [Fact]
        public async Task Delete_CategoryNotExists_ReturnsNotFound()
        {
            // Arrange
            const int NotExistingCategoryId = 999;
            _serviceMock
                .Setup(s => s.DeleteAsync(NotExistingCategoryId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(NotExistingCategoryId);

            // Assert
            Assert.Equal("Category not found", result.Error?.Message);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
        }
    }
}

