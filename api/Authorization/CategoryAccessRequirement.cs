using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Represents a requirement for accessing a category resource.
    /// </summary>
    /// <remarks>
    /// This requirement is evaluated by <see cref="CategoryAccessHandler"/> to determine whether a user
    /// can access a given category, optionally allowing access to global (common) categories.
    /// </remarks>
    public class CategoryAccessRequirement : IAuthorizationRequirement
    {
    }
}