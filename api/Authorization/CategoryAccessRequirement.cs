using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Represents a requirement for accessing a category resource.
/// </summary>
/// <remarks>
/// This requirement is evaluated by <see cref="CategoryAccessHandler"/> to determine whether a user
/// can access a given category, optionally allowing access to global (common) categories.
/// </remarks>
public class CategoryAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryAccessRequirement"/> class.
    /// </summary>
    /// <param name="allowGlobal">
    /// If set to <c>true</c>, access to global (common) categories is allowed.
    /// Defaults to <c>false</c>.
    /// </param>
    public CategoryAccessRequirement(bool allowGlobal = false)
    {
        AllowGlobal = allowGlobal;
    }

    /// <summary>
    /// Gets a value indicating whether access to global (common) categories is allowed.
    /// </summary>
    public bool AllowGlobal { get; }
}
