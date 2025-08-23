using api.Services.Common;
using System.Security.Claims;

namespace api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static CurrentUser ToCurrentUser(this ClaimsPrincipal user)
        {
            return new CurrentUser
            {
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                IsAdmin = user.IsInRole("Admin"),
                Roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList(),
            };
        }
    }
}