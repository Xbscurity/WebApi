using api.Data;
using api.Dtos.Category;
using api.Extensions;
using api.Migrations;
using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Services.Categories;
using api.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MockQueryable.Moq;
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
            var includeInactive = false;
            var categoriesMock = new List<Category>()
            {
                new() { Id = 1, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 2, Name = "Active", AppUserId = TestUserId, IsActive = true },

                new() { Id = 3, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 4, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },

            }.AsQueryable().BuildMockDbSet();

            _categoryRepositoryMock.Setup(r => r.GetQueryable(includeInactive)).
                Returns(categoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(
                TestUserId, queryObject, includeInactive: false);

            // Assert
            var categoriesCount = categoriesMock.Object.Count(
                x => x.AppUserId == TestUserId);

            Assert.Equal(categoriesCount, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(includeInactive), Times.Once);
        }

        [Fact]
        public async Task GetAllForUserAsync_IncludeInactiveCategories_ReturnsAllOwnCategories()
        {
            var includeInactive = true;
            // Arrange
            var categoriesMock = new List<Category>()
            {
                new() { Id = 1, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 2, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 3, Name = "Inactive", AppUserId = TestUserId, IsActive = false },
                new() { Id = 4, Name = "Inactive", AppUserId = TestUserId, IsActive = false },

                new() { Id = 5, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 6, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 7, Name = "Other user Inactive", AppUserId = OtherUserId, IsActive = false },
                new() { Id = 8, Name = "Other user Inactive", AppUserId = OtherUserId, IsActive = false },

            }.AsQueryable().BuildMockDbSet();

            _categoryRepositoryMock.Setup(r => r.GetQueryable(includeInactive)).Returns(categoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(
                TestUserId, queryObject, includeInactive: true);

            // Assert
            var categoriesCount = categoriesMock.Object.Count(x => x.AppUserId == TestUserId);

            Assert.Equal(categoriesCount, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);
            Assert.Contains(result.Data, c => c.Id == 3);
            Assert.Contains(result.Data, c => c.Id == 4);

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(includeInactive), Times.Once);
        }

        [Fact]
        public async Task GetAllForUserAsync_NoCategories_ReturnsEmptyList()
        {
            // Arrange
            var categoriesMock = new List<Category>().AsQueryable().BuildMockDbSet();

            _categoryRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<bool>())).Returns(categoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForUserAsync(TestUserId, queryObject, includeInactive: true);

            // Assert

            var categoriesCount = categoriesMock.Object.Count();

            Assert.NotNull(result);
            Assert.Empty(result.Data);

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_WithoutUserId_ReturnsAllCategories()
        {
            // Arrange
            var categoriesMock = new List<Category>()
            {
                new() { Id = 1, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 2, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 3, Name = "Active", AppUserId = TestUserId, IsActive = false },
                new() { Id = 4, Name = "Active", AppUserId = TestUserId, IsActive = false },

                new() { Id = 5, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 6, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 7, Name = "Other user Active", AppUserId = OtherUserId, IsActive = false },
                new() { Id = 8, Name = "Other user Active", AppUserId = OtherUserId, IsActive = false },

            }.AsQueryable().BuildMockDbSet();

            _categoryRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<bool>())).Returns(categoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: null);

            // Assert
            var categoriesCount = categoriesMock.Object.Count();

            var expectedIds = Enumerable.Range(1, 8);
            Assert.All(expectedIds, id => Assert.Contains(result.Data, c => c.Id == id));

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_WithUserId_ReturnsOnlyTestUserIdCategories()
        {
            // Arrange
            var categoriesMock = new List<Category>()
            {
                new() { Id = 1, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 2, Name = "Active", AppUserId = TestUserId, IsActive = true },
                new() { Id = 3, Name = "Active", AppUserId = TestUserId, IsActive = false },
                new() { Id = 4, Name = "Active", AppUserId = TestUserId, IsActive = false },

                new() { Id = 5, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 6, Name = "Other user Active", AppUserId = OtherUserId, IsActive = true },
                new() { Id = 7, Name = "Other user Active", AppUserId = OtherUserId, IsActive = false },
                new() { Id = 8, Name = "Other user Active", AppUserId = OtherUserId, IsActive = false },

            }.AsQueryable().BuildMockDbSet();

            _categoryRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<bool>())).Returns(categoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            var userId = TestUserId;
            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: userId);

            // Assert
            var categoriesCount = categoriesMock.Object.Count(x => x.AppUserId == userId);

            Assert.Equal(categoriesCount, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal(TestUserId, c.AppUserId));

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            _categoryRepositoryMock.Verify(r => r.GetQueryable(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdminAsync_NoCategories_ReturnsEmptyList()
        {
            // Arrange
            var emptyCategoriesMock = new List<Category>().AsQueryable().BuildMockDbSet();
            _categoryRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<bool>()))
                .Returns(emptyCategoriesMock.Object);

            var queryObject = new PaginationQueryObject { Page = 1, Size = 10 };

            // Act
            var result = await _categoryService.GetAllForAdminAsync(queryObject, userId: null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);

            var categoriesCount = emptyCategoriesMock.Object.Count();

            Assert.Equal(categoriesCount, result.Pagination.TotalItems);
            Assert.False(result.Pagination.HasNext);
            Assert.False(result.Pagination.HasPrevious);
            Assert.Equal(queryObject.Page, result.Pagination.PageNumber);
            Assert.Equal(queryObject.Size, result.Pagination.PageSize);

            // Verify
            _categoryRepositoryMock.Verify(r => r.GetQueryable(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateInitialCategoriesForUserAsync_ValidUserId_CreatesDefaultCategories()
        {
            // Arrange

            // Act
            await _categoryService.CreateInitialCategoriesForUserAsync(TestUserId);

            // Assert

            var expectedCount = DataSeeder.DefaultCategoryTemplates.Count;
            var expectedNames = DataSeeder.DefaultCategoryTemplates.Select(t => t.Name).ToList();

            _categoryRepositoryMock.Verify(r => r.CreateRangeAsync(
                It.Is<List<Category>>(actualCategories =>
                    // 1. Check the count
                    actualCategories.Count == expectedCount &&
                    // 2. Check the UserId and IsActive flag for all created categoriesMock
                    actualCategories.All(c => c.AppUserId == TestUserId && c.IsActive == true) &&
                    // 3. Check that all template names are present in the created categoriesMock
                    expectedNames.All(name => actualCategories.Any(c => c.Name == name))
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_CategoryExists_CallsRepositoryDelete()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test" };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<bool>()))
                     .ReturnsAsync(category);

            // Act
            await _categoryService.DeleteAsync(category.Id);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(category.Id, It.IsAny<bool>()), Times.Once);

            _categoryRepositoryMock.Verify(r => r.DeleteAsync(
                It.Is<Category>(c => c.Id == category.Id)),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_CategoryExists_ReturnsCorrectUpdatedDto()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Old" };
            var inputDto = new BaseCategoryUpdateInputDto { Name = "  New Name  " };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<bool>()))
                    .ReturnsAsync(category);

            // Act
            var result = await _categoryService.UpdateAsync(category.Id, inputDto);

            // Assert
            var expectedName = inputDto.Name.Trim();

            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(category.Id, It.IsAny<bool>()), Times.Once);

            _categoryRepositoryMock.Verify(r => r.UpdateAsync(
                It.Is<Category>(c => c.Name == expectedName && c.Id == category.Id)),
                Times.Once);
        }
        [Fact]
        public async Task CreateForAdminAsync_ValidInput_ReturnsCorrectDtoWithTrimmedName()
        {
            // Arrange
            var inputDto = new AdminCategoryCreateInputDto
            {
                Name = "  New Category  ",
                AppUserId = TestUserId,
            };

            var expectedName = inputDto.Name.Trim();

            _categoryRepositoryMock.Setup(r => r.CreateAsync(It.Is<Category>(
                c => c.AppUserId == inputDto.AppUserId && c.Name == expectedName)))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.CreateForAdminAsync(inputDto);

            // Assert

            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.Is<Category>(
                c => c.AppUserId == inputDto.AppUserId && c.Name == expectedName)),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(inputDto.AppUserId, result.AppUserId);
        }

        [Fact]
        public async Task CreateForUserAsync_ValidInput_ReturnsCorrectDtoWithTrimmedName()
        {
            // Arrange
            var inputDto = new BaseCategoryUpdateInputDto
            {
                Name = "  New Category  ",
            };
            var expectedName = inputDto.Name.Trim();

            _categoryRepositoryMock.Setup(r => r.CreateAsync(It.Is<Category>(
                c => c.Name == expectedName && c.AppUserId == TestUserId)))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.CreateForUserAsync(TestUserId, inputDto);

            // Assert
            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once);


            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(TestUserId, result.AppUserId);
        }
        [Fact]
        public async Task GetByIdAsync_CategoryExists_ReturnsCorrectDto()
        {
            // Arrange
            var existingCategory = new Category { Id = 1, Name = "Test" };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(existingCategory.Id, It.IsAny<bool>()))
                .ReturnsAsync(existingCategory);     

            // Act
            var result = await _categoryService.GetByIdAsync(existingCategory.Id);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(existingCategory.Id, It.IsAny<bool>()), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(existingCategory.Id, result.Id);
            Assert.Equal(existingCategory.Name, result.Name);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ToggleActiveAsync_TogglesState_ReturnsCorrectFlippedState(bool isActive)
        {
            // Arrange
            var existingCategory = new Category 
            {
                Id = 1,
                IsActive = isActive,
            };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(existingCategory.Id, It.IsAny<bool>()))
                .ReturnsAsync(existingCategory);

            // Act
            var result = await _categoryService.ToggleActiveAsync(existingCategory.Id);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(existingCategory.Id,
                It.IsAny<bool>()), Times.Once);

            _categoryRepositoryMock.Verify(r => r.UpdateAsync(existingCategory),
                Times.Once);

            Assert.Equal(!isActive, existingCategory.IsActive);
        }
    }
}
