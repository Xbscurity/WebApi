using api.Constants;
using api.Controllers.Category;
using api.Models;
using api.Services.Categories;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace api.Tests.Unit.Controllers
{
    public class AdminCategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly CategorySortValidator _sortValidator;
        private readonly ILogger<AdminCategoryController> _logger;
        private readonly AdminCategoryController _controller;

        public AdminCategoryControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _authServiceMock = new Mock<IAuthorizationService>();
            _sortValidator = new CategorySortValidator();
            _logger = NullLogger<AdminCategoryController>.Instance;

            _controller = new AdminCategoryController(
                _categoryServiceMock.Object,
                _logger,
                _sortValidator,
                _authServiceMock.Object
            );
        }
        public async Task GetById_ExistingAndAuthorizedCategory_ReturnsSuccessWithExistingCategory()
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
            };
            var authSuccess = AuthorizationResult.Success();

            _categoryServiceMock.Setup(c => c.GetByIdRawAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            _authServiceMock.
                Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     existingCategory,
                     Policies.CategoryAccessGlobal)).
                ReturnsAsync(authSuccess);

            // Act
            var result = await _controller.GetById(existingCategory.Id);

            // Assert
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.Equal(existingCategory.Id, result.Data.Id);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(existingCategory.Id), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccessGlobal), Times.Once);
        }

        public async Task GetById_NotExistingCategory_ReturnsNotFound()
        {
            // Arrange
            var notExistingId = 999;

            _categoryServiceMock.Setup(s => s.GetByIdRawAsync(notExistingId)).ReturnsAsync((Category)null!);
            // Act
            var result = await _controller.GetById(notExistingId);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
            Assert.Equal("Category not found", result.Error.Message);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(notExistingId), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<Category>(),
                It.IsAny<string>()),
                Times.Never);
        }

        public async Task GetById_ExistingAndUnAuthorizedCategory_ReturnsNotFound()
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
            };
            var authFailed = AuthorizationResult.Failed();

            _categoryServiceMock.Setup(c => c.GetByIdRawAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            _authServiceMock.
                Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     existingCategory,
                     Policies.CategoryAccessGlobal)).
                ReturnsAsync(authFailed);

            // Act
            var result = await _controller.GetById(existingCategory.Id);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
            Assert.Equal("Category not found", result.Error.Message);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(existingCategory.Id), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccessGlobal), Times.Once);
        }
        public async Task Delete_ExistingAndAuthorizedCategory_ReturnsSuccessWithTrue()
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
            };
            var authSuccess = AuthorizationResult.Success();

            _categoryServiceMock.Setup(c => c.GetByIdRawAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            _authServiceMock.
                Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     existingCategory,
                     Policies.CategoryAccessNoGlobal)).
                ReturnsAsync(authSuccess);

            _categoryServiceMock.Setup(c => c.DeleteAsync(existingCategory.Id)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(existingCategory.Id);

            // Assert
            Assert.Null(result.Error);
            Assert.True(result.Data);
            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(existingCategory.Id), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccessGlobal), Times.Once);

            _categoryServiceMock.Verify(s => s.DeleteAsync(existingCategory.Id), Times.Once);
        }

        public async Task Delete_NotExistingCategory_ReturnsNotFound()
        {
            // Arrange
            var notExistingId = 999;

            _categoryServiceMock.Setup(s => s.GetByIdRawAsync(notExistingId)).ReturnsAsync((Category)null!);
            // Act
            var result = await _controller.Delete(notExistingId);

            // Assert
            Assert.False(result.Data);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
            Assert.Equal("Category not found", result.Error.Message);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(notExistingId), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<Category>(),
                It.IsAny<string>()),
                Times.Never);
        }

        public async Task Delete_ExistingAndUnAuthorizedUserCategory_ReturnsUnauthorized()
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
            };
            var authFailed = AuthorizationResult.Failed();

            _categoryServiceMock.Setup(c => c.GetByIdRawAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            _authServiceMock.
                Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     existingCategory,
                     Policies.CategoryAccessNoGlobal)).
                ReturnsAsync(authFailed);

            // Act
            var result = await _controller.Delete(existingCategory.Id);

            // Assert
            Assert.False(result.Data);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.Unauthorized, result.Error.Code);
            Assert.Equal("Forbidden access to category", result.Error.Message);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(existingCategory.Id), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccessNoGlobal), Times.Once);

            _categoryServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        public async Task Delete_ExistingAndUnAuthorizedCommonCategory_ReturnsUnauthorized()
        {
            // Arrange
            var existingCategory = new Category
            {
                Id = 1,
            };
            var authFailed = AuthorizationResult.Failed();

            _categoryServiceMock.Setup(c => c.GetByIdRawAsync(existingCategory.Id)).ReturnsAsync(existingCategory);

            _authServiceMock.
                Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     existingCategory,
                     Policies.CategoryAccessNoGlobal)).
                ReturnsAsync(authFailed);

            // Act
            var result = await _controller.Delete(existingCategory.Id);

            // Assert
            Assert.False(result.Data);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.Unauthorized, result.Error.Code);
            Assert.Equal("Cannot modify common categories", result.Error.Message);

            _categoryServiceMock.Verify(s => s.GetByIdRawAsync(existingCategory.Id), Times.Once);

            _authServiceMock.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccessNoGlobal), Times.Once);

            _categoryServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
