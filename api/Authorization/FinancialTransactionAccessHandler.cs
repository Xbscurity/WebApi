using api.Constants;
using api.Extensions;
using api.Models;
using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Handles authorization requirements for accessing <see cref="FinancialTransaction"/> resources.
    /// </summary>
    public class FinancialTransactionAccessHandler : AuthorizationHandler<FinancialTransactionAccessRequirement, FinancialTransaction>
    {
        private readonly ILogger<FinancialTransactionAccessHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAccessHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record authorization checks.</param>
        public FinancialTransactionAccessHandler(ILogger<FinancialTransactionAccessHandler> logger)
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
            FinancialTransactionAccessRequirement requirement,
            FinancialTransaction transaction)
        {
            var userId = context.User.GetUserId();

            bool isOwner = transaction.AppUserId == userId;

            _logger.LogDebug(
                "Transaction authorization check: UserId={UserId}, TransactionId={TransactionId}, " +
                "IsOwner={IsOwner}",
                userId,
                transaction.Id,
                isOwner);

            if (isOwner)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
