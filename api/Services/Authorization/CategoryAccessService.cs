using api.Constants;
using api.Models;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace api.Services.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="ICategoryAccessService"/>.
    /// </summary>
    /// <remarks>
    /// This service validates whether the current authenticated user
    /// is permitted to access a specific category
    /// using the configured authorization policies.
    /// </remarks>
    public class CategoryAccessService : ICategoryAccessService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _context;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CategoryAccessService"/> class.
        /// </summary>
        /// <param name="context">
        /// The HTTP context accessor used to retrieve the current user context.
        /// </param>
        /// <param name="authorizationService">
        /// The authorization service used to evaluate access policies.
        /// </param>
        public CategoryAccessService(
            IAuthorizationService authorizationService,
            IHttpContextAccessor context)
        {
            _authorizationService = authorizationService;
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<Success>> CanAccessCheckAsync(Category category)
        {
            if (_context.HttpContext == null)
            {
                return Errors.Category.AccessDenied(category.Id);
            }

            var authResult = await _authorizationService.AuthorizeAsync(
                _context.HttpContext.User,
                category,
                Policies.CategoryAccess);

            if (!authResult.Succeeded)
            {
                return Errors.Category.AccessDenied(category.Id);
            }

            return Result.Success;
        }
    }
}