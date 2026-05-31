using api.Models;
using ErrorOr;

namespace api.Services.Authorization
{
    /// <summary>
    /// Defines authorization checks for accessing category.
    /// </summary>
    public interface ICategoryAccessService
    {
        /// <summary>
        /// Verifies whether the current user is authorized
        /// to access the specified category.
        /// </summary>
        /// <param name="category">
        /// The category to validate access for.
        /// </param>
        /// <returns>
        /// A <see cref="Success"/> result when access is granted;
        /// otherwise, an authorization error.
        /// </returns>
        Task<ErrorOr<Success>> CanAccessCheckAsync(Category category);
    }
}
