using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Represents a custom authorization requirement that ensures a user is not banned.
    /// </summary>
    /// <remarks>
    /// This requirement is used in conjunction with <see cref="NotBannedHandler"/>
    /// to enforce that only users who are not banned can access certain resources.
    /// </remarks>
    public class NotBannedRequirement : IAuthorizationRequirement
    {
    }
}
