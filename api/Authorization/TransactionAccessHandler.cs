using api.Constants;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.Authorization
{
    public class TransactionAccessHandler : AuthorizationHandler<TransactionAccessRequirement, FinancialTransaction>
    {
        private readonly ILogger<CategoryAccessHandler> _logger;

        public TransactionAccessHandler(ILogger<CategoryAccessHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TransactionAccessRequirement requirement,
            FinancialTransaction transaction)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = context.User.IsInRole(Roles.Admin);

            bool isOwner = transaction.AppUserId == userId;

            _logger.LogDebug(
                "Transaction authorization check: UserId={UserId}, Transaction Id={TransactionId}," +
                " IsAdmin={IsAdmin}, IsOwner={IsOwner}",
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
