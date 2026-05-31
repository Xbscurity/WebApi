using api.Constants;
using api.Extensions;

namespace api.Providers.CurrentUser
{
    /// <summary>
    /// Default implementation of <see cref="ICurrentUser"/>.
    /// </summary>
    /// <remarks>
    /// This implementation retrieves user information from
    /// <see cref="HttpContext.User"/>.
    /// </remarks>
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUser"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">
        /// The accessor used to retrieve the current HTTP context.
        /// </param>
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string UserId => _httpContextAccessor.HttpContext?.User?.GetUserId()
            ?? throw new InvalidOperationException(
                "Attempted to access UserId outside of a valid authenticated request context.");

        /// <inheritdoc />
        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole(Roles.Admin) ?? false;
    }
}
