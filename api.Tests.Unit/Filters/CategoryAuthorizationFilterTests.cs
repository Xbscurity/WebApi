using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Filters;
using api.Models;
using api.Repositories.Categories;
using api.Responses;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace api.Tests.Unit.Filters
{
    public class CategoryAuthorizationFilterTests
    {
        private readonly Mock<IAuthorizationService> _mockAuthService;
        private readonly Mock<ILogger<CategoryAuthorizationFilter>> _mockLogger;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryAuthorizationFilter _filter;

        public CategoryAuthorizationFilterTests()
        {
            _mockAuthService = new Mock<IAuthorizationService>();
            _mockLogger = new Mock<ILogger<CategoryAuthorizationFilter>>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();

            var actionContext = FilterTestHelper.CreateActionContext();

            _filter = new CategoryAuthorizationFilter(
                _mockAuthService.Object,
                _mockLogger.Object,
                _mockCategoryRepository.Object);
        }

        private static ActionExecutingContext CreateContext(object? id = null, string argumentName = "id")
        {
            var actionContext = FilterTestHelper.CreateActionContext();

            var actionArguments = new Dictionary<string, object?>();
            if (id != null)
                actionArguments[argumentName] = id;

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: new object()
            );
        }
        [Fact]
        public async Task OnActionExecutionAsync_MissingParameter_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateContext(null);

            var nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.False(nextCalled);

            var result = Assert.IsType<BadRequestObjectResult>(context.Result);

            Assert.IsType<ApiResponse<object>>(result.Value);

            _mockCategoryRepository.Verify(
                r => r.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>()),
                Times.Never);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()),
                Times.Never);

        }

        [Fact]
        public async Task OnActionExecutionAsync_InvalidParameterType_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateContext("not-an-int-or-dto");

            var nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.False(nextCalled);

            var result = Assert.IsType<BadRequestObjectResult>(context.Result);

            Assert.IsType<ApiResponse<object>>(result.Value);

            _mockCategoryRepository.Verify(
                r => r.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>()),
                Times.Never);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_CategoryNotFound_ReturnsNotFound()
        {
            // Arrange
            int categoryId = 999;

            var context = CreateContext(categoryId);

            var nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            _mockCategoryRepository.Setup(
                r => r.GetByIdAsync(categoryId, false))
                .ReturnsAsync((Category?)null);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.False(nextCalled);

            var result = Assert.IsType<NotFoundObjectResult>(context.Result);

            Assert.IsType<ApiResponse<object>>(result.Value);

            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId, false), Times.Once);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()),
                Times.Never);

        }

        [Fact]
        public async Task OnActionExecutionAsync_AuthorizationFailed_ReturnsForbidden()
        {
            // Arrange
            var existingCategory = new Category { Id = 1, Name = "Test Category" };

            var context = CreateContext(existingCategory.Id);

            bool nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            _mockCategoryRepository.Setup(
                r => r.GetByIdAsync(
                    existingCategory.Id, false)).
                    ReturnsAsync(existingCategory);

            _mockAuthService.Setup(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess)).
                    ReturnsAsync(AuthorizationResult.Failed());

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.False(nextCalled);

            var result = Assert.IsType<ObjectResult>(context.Result);

            Assert.Equal(403, result.StatusCode);

            Assert.IsType<ApiResponse<object>>(result.Value);

            _mockCategoryRepository.Verify(r => r.GetByIdAsync(existingCategory.Id, false), Times.Once);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess),
                Times.Once);
        }

        [Fact]
        public async Task OnActionExecutionAsync_ParameterIsIHasCategoryIdDto_ExtractsIdCorrectlyAndCallsNext()
        {
            // Arrange
            var existingCategory = new Category { Id = 1, Name = "Test Category" };

            var dto = new BaseFinancialTransactionInputDto
            {
                CategoryId = 1,
                Amount = 100,
                Comment = "Test"
            };
            var context = CreateContext(dto);

            bool nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };


            _mockCategoryRepository.Setup(r => r.GetByIdAsync(existingCategory.Id, false))
                .ReturnsAsync(existingCategory);

            _mockAuthService.Setup(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess)).
                    ReturnsAsync(AuthorizationResult.Success());

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Null(context.Result);

            Assert.True(nextCalled);

            _mockCategoryRepository.Verify(r => r.GetByIdAsync(existingCategory.Id, false), Times.Once);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess),
                Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnActionExecutionAsync_IncludeInactiveConfigured_PassesCorrectFlagToRepositoryAndCallsNext(
            bool includeInactive)
        {
            // Arrange
            var existingCategory = new Category { Id = 1, Name = "Test Category" };

            var context = CreateContext(existingCategory.Id);

            bool nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            var includeInactiveFilter = new CategoryAuthorizationFilter(
                _mockAuthService.Object,
                _mockLogger.Object,
                _mockCategoryRepository.Object,
                includeInactive: includeInactive);

            _mockCategoryRepository.Setup(r => r.GetByIdAsync(existingCategory.Id, includeInactive)).
                ReturnsAsync(existingCategory);

            _mockAuthService.Setup(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccess))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            await includeInactiveFilter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Null(context.Result);

            Assert.True(nextCalled);

            _mockCategoryRepository.Verify(
                r => r.GetByIdAsync(existingCategory.Id, includeInactive),
                Times.Once);

            _mockAuthService.Verify(s => s.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                existingCategory,
                Policies.CategoryAccess),
                Times.Once);
        }

        [Theory]
        [InlineData("custom_id")]
        [InlineData("id")]  
        public async Task OnActionExecutionAsync_AuthorizationSucceeded_CallsNext(string argumentName)
        {
            // Arrange
            var filter = new CategoryAuthorizationFilter(
                _mockAuthService.Object,
                _mockLogger.Object,
                _mockCategoryRepository.Object,
                parameterName: argumentName);

            var existingCategory = new Category { Id = 1, Name = "Test Category" };


            var context = CreateContext(existingCategory.Id, argumentName);

            bool nextCalled = false;

            ActionExecutionDelegate next = () =>
            {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            _mockCategoryRepository.Setup(
                r => r.GetByIdAsync(
                    existingCategory.Id, false)).
                    ReturnsAsync(existingCategory);

            _mockAuthService.Setup(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess))
                .ReturnsAsync(AuthorizationResult.Success());


            // Act
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Null(context.Result);

            Assert.True(nextCalled);

            _mockCategoryRepository.Verify(
                r => r.GetByIdAsync(
                    existingCategory.Id, false),
                Times.Once);

            _mockAuthService.Verify(
                s => s.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    existingCategory,
                    Policies.CategoryAccess),
                Times.Once);
        }   
    }
}