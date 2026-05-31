using Microsoft.AspNetCore.Authorization;

namespace api.Constants
{
    /// <summary>
    /// Defines authorization policy names used across the application.
    /// </summary>
    /// <remarks>
    /// These policies are registered in the authorization configuration
    /// and used via <see cref="AuthorizeAttribute"/> to protect endpoints
    /// based on business rules.
    /// </remarks>
    public static class Policies
    {
        /// <summary>
        /// Policy that allows access only to users who are not banned.
        /// </summary>
        public const string NotBanned = nameof(NotBanned);

        /// <summary>
        /// Policy that enforces access control for category-related operations.
        /// </summary>
        public const string CategoryAccess = nameof(CategoryAccess);

        /// <summary>
        /// Policy that enforces access control for financial transaction operations.
        /// </summary>
        public const string FinancialTransactionAccess = nameof(FinancialTransactionAccess);
    }
}
