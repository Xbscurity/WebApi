using System.Security.Claims;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for working with
    /// <see cref="ClaimsPrincipal"/> instances.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the user identifier from the claims principal.
        /// </summary>
        /// <param name="user">
        /// The claims principal containing user claims.
        /// </param>
        /// <returns>
        /// The user identifier extracted from the
        /// <see cref="ClaimTypes.NameIdentifier"/> claim,
        /// or <see langword="null"/> if the claim is not present.
        /// </returns>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}