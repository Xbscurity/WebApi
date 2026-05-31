using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Authorization requirement that ensures a user is not banned.
    /// </summary>
    /// <remarks>
    /// This is used by the <c>NotBanned</c> policy to restrict access
    /// for users whose accounts have been flagged as banned.
    /// </remarks>
    public class NotBannedRequirement : IAuthorizationRequirement
    {
    }
}