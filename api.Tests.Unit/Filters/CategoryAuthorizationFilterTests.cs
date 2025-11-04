using api.Constants;
using api.Filters;
using api.Models;
using api.Repositories.Categories;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

public class CategoryAuthorizationFilterTests
{
    private readonly Mock<IAuthorizationService> _authServiceMock;
    private readonly Mock<ILogger<CategoryAuthorizationFilter>> _loggerMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly string _policy;
    private readonly CategoryAuthorizationFilter _filter;

    public CategoryAuthorizationFilterTests()
    {
        _authServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<CategoryAuthorizationFilter>>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _policy = Policies.CategoryAccess;
        _filter = new CategoryAuthorizationFilter(
            _authServiceMock.Object,
            _loggerMock.Object,
            _categoryRepositoryMock.Object,
            _policy
        );
    }
    private static ActionExecutingContext CreateContext(object? id = null)
    {
        var actionContext = FilterTestHelper.CreateActionContext();

        var actionArguments = new Dictionary<string, object?>();
        if (id != null)
            actionArguments["id"] = id;

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            controller: null
        );
    }
    [Theory]
    [InlineData(null)]
    [InlineData("not_an_int")]
    public async Task OnActionExecutionAsync_InvalidOrMissingId_ReturnsBadRequest(object? id)
    {
        // Arrange
        var context = CreateContext(id);
        var next = new Mock<ActionExecutionDelegate>();

        // Act
        await _filter.OnActionExecutionAsync(context, next.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(context.Result);

        _categoryRepositoryMock.Verify(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        _authServiceMock.Verify(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()),
            Times.Never
        );
        next.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_CategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistingCategoryId = 999;
        var context = CreateContext(nonExistingCategoryId);
        _categoryRepositoryMock.Setup(s => s.GetByIdAsync(nonExistingCategoryId, It.IsAny<bool>())).ReturnsAsync((Category?)null);

        var next = new Mock<ActionExecutionDelegate>();

        // Act
        await _filter.OnActionExecutionAsync(context, next.Object);

        // Assert
        Assert.IsType<NotFoundObjectResult>(context.Result);

        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(nonExistingCategoryId, It.IsAny<bool>()), Times.Once);
        _authServiceMock.Verify(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()),
            Times.Never
        );
        next.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_CategoryAccessPolicyFails_ReturnsForbidden()
    {
        // Arrange

        var nonCommonCategory = new Category
        {
            Id = 1,
        };

        var context = CreateContext(nonCommonCategory.Id);

        _categoryRepositoryMock.Setup(s => s.GetByIdAsync(nonCommonCategory.Id, It.IsAny<bool>())).ReturnsAsync(nonCommonCategory);

        var failedAuthResult = AuthorizationResult.Failed();

        _authServiceMock.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            nonCommonCategory,
            _policy))
            .ReturnsAsync(failedAuthResult);

        var next = new Mock<ActionExecutionDelegate>();

        // Act
        await _filter.OnActionExecutionAsync(context, next.Object);

        // Assert
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(nonCommonCategory.Id, It.IsAny<bool>()), Times.Once);

        _authServiceMock.Verify(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            nonCommonCategory,
            _policy),
            Times.Once
        );

        next.Verify(x => x(), Times.Never);

        Assert.NotNull(context.Result);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);

        Assert.Equal(403, objectResult.StatusCode);
    }
    [Fact]
    public async Task OnActionExecutionAsync_AuthorizationSucceeds_CallsNextDelegate()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
        };

        var context = CreateContext(category.Id);

        _categoryRepositoryMock
            .Setup(s => s.GetByIdAsync(category.Id, It.IsAny<bool>()))
            .ReturnsAsync(category);

        var successfulAuthResult = AuthorizationResult.Success();
        _authServiceMock
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                category,
                It.IsAny<string>()))
            .ReturnsAsync(successfulAuthResult);

        var next = new Mock<ActionExecutionDelegate>();

        // Act
        await _filter.OnActionExecutionAsync(context, next.Object);

        // Assert
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(category.Id, It.IsAny<bool>()), Times.Once);
        _authServiceMock.Verify(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            category,
            It.IsAny<string>()), Times.Once);

        next.Verify(x => x(), Times.Once);

        Assert.Null(context.Result);
    }
}