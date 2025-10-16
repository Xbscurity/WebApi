using api.Constants;
using api.Filters;
using api.Models;
using api.Services.Categories;
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
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly string _policy;
    private readonly CategoryAuthorizationFilter _filter;

    public CategoryAuthorizationFilterTests()
    {
        _authServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<CategoryAuthorizationFilter>>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _policy = Policies.CategoryAccess;
        _filter = new CategoryAuthorizationFilter(
            _authServiceMock.Object,
            _loggerMock.Object,
            _categoryServiceMock.Object,
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

        _categoryServiceMock.Verify(s => s.GetByIdRawAsync(It.IsAny<int>()), Times.Never);
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
        _categoryServiceMock.Setup(s => s.GetByIdRawAsync(nonExistingCategoryId)).ReturnsAsync((Category?)null);

        var next = new Mock<ActionExecutionDelegate>();

        // Act
        await _filter.OnActionExecutionAsync(context, next.Object);

        // Assert
        Assert.IsType<NotFoundObjectResult>(context.Result);

        _categoryServiceMock.Verify(x => x.GetByIdRawAsync(nonExistingCategoryId), Times.Once);
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

        _categoryServiceMock.Setup(s => s.GetByIdRawAsync(nonCommonCategory.Id)).ReturnsAsync(nonCommonCategory);

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
        _categoryServiceMock.Verify(x => x.GetByIdRawAsync(nonCommonCategory.Id), Times.Once);

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

        _categoryServiceMock
            .Setup(s => s.GetByIdRawAsync(category.Id))
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
        _categoryServiceMock.Verify(x => x.GetByIdRawAsync(category.Id), Times.Once);
        _authServiceMock.Verify(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            category,
            It.IsAny<string>()), Times.Once);

        next.Verify(x => x(), Times.Once);

        Assert.Null(context.Result);
    }
}