using api.Constants;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class CategoryAccessHandler : AuthorizationHandler<CategoryAccessRequirement, Category>
{
    private readonly ILogger<CategoryAccessHandler> _logger;

    public CategoryAccessHandler(ILogger<CategoryAccessHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CategoryAccessRequirement requirement,
        Category category)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole(Roles.Admin);

        bool isOwner = category.AppUserId == userId;
        bool includeGlobal = requirement.AllowGlobal && category.AppUserId == null;

        _logger.LogDebug(
            "Authorization check: UserId={UserId}, CategoryId={CategoryId}," +
            " IsAdmin={IsAdmin}, IsOwner={IsOwner}, AllowGlobal={AllowGlobal}, IncludeGlobal={IncludeGlobal}",
            userId,
            category.Id,
            isAdmin,
            isOwner,
            requirement.AllowGlobal,
            includeGlobal);
        if (isAdmin || isOwner || includeGlobal)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
