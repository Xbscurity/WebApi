using api.Constants;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.Authorization
{
    /// <summary>
    /// Handles authorization requirements for accessing <see cref="FinancialTransaction"/> resources.
    /// </summary>
    /// <remarks>
    /// This handler checks whether the current user is either an administrator
    /// or the owner of the requested transaction.
    /// </remarks>
    public class TransactionAccessHandler : AuthorizationHandler<TransactionAccessRequirement, FinancialTransaction>
    {
        private readonly ILogger<TransactionAccessHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAccessHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record authorization checks.</param>
        public TransactionAccessHandler(ILogger<TransactionAccessHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Makes an authorization decision for the specified <see cref="FinancialTransaction"/>.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The transaction access requirement being evaluated.</param>
        /// <param name="transaction">The transaction resource to authorize.</param>
        /// <returns>A completed <see cref="Task"/> representing the operation.</returns>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TransactionAccessRequirement requirement,
            FinancialTransaction transaction)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = context.User.IsInRole(Roles.Admin);

            bool isOwner = transaction.AppUserId == userId;

            _logger.LogDebug(
                "Transaction authorization check: UserId={UserId}, TransactionId={TransactionId}, " +
                "IsAdmin={IsAdmin}, IsOwner={IsOwner}",
                userId,
                transaction.Id,
                isAdmin,
                isOwner);

            if (isAdmin || isOwner)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
