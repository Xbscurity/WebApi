using System.Security.Claims;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for working with <see cref="ClaimsPrincipal"/> instances.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the user identifier (<see cref="ClaimTypes.NameIdentifier"/>)
        /// from the specified <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
        /// <returns>
        /// The user identifier as a <see cref="string"/> if present; otherwise, <see langword="null"/>.
        /// </returns>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}