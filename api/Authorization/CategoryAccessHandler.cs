using api.Constants;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

/// <summary>
/// Authorization handler that determines whether a user has access to a given <see cref="Category"/>.
/// </summary>
public class CategoryAccessHandler : AuthorizationHandler<CategoryAccessRequirement, Category>
{
    private readonly ILogger<CategoryAccessHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryAccessHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write access check details.</param>
    public CategoryAccessHandler(ILogger<CategoryAccessHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Makes an authorization decision for the specified <paramref name="category"/> based on the user
    /// and the provided <paramref name="requirement"/>.
    /// </summary>
    /// <param name="context">The authorization context, including the current user principal.</param>
    /// <param name="requirement">The category access requirement being evaluated.</param>
    /// <param name="category">The category resource to which access is being checked.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Grants access if the user is an administrator, is the owner of the category.
    /// </remarks>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CategoryAccessRequirement requirement,
        Category category)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole(Roles.Admin);

        bool isOwner = category.AppUserId == userId;

        _logger.LogDebug(
            "Category access check: UserId={UserId}, CategoryId={CategoryId}," +
            " IsAdmin={IsAdmin}, IsOwner={IsOwner}",
            userId,
            category.Id,
            isAdmin,
            isOwner);
        if (isAdmin || isOwner)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
