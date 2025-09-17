using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Represents a requirement for accessing a financial transaction resource.
    /// </summary>
    /// <remarks>
    /// This requirement is evaluated by <see cref="TransactionAccessHandler"/> to determine whether a user
    /// can access a given financial transaction.
    /// </remarks>
    public class TransactionAccessRequirement : IAuthorizationRequirement
    {
    }
}
