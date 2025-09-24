using api.Constants;
using api.Extensions;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class FinancialTransactionAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<FinancialTransactionAuthorizationFilter> _logger;

        public FinancialTransactionAuthorizationFilter(
           IAuthorizationService authorizationService,
           ILogger<FinancialTransactionAuthorizationFilter> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idValue) || !(idValue is int financialTransactionId))
            {
                context.Result = new BadRequestObjectResult(ApiResponse.BadRequest<object>());
                return;
            }

            var financialTransactionService = context.HttpContext.RequestServices.GetRequiredService<IFinancialTransactionService>();
            var financialTransaction = await financialTransactionService.GetByIdRawAsync(financialTransactionId);

            if (financialTransaction == null)
            {
                context.Result = new NotFoundObjectResult(ApiResponse.NotFound<object>($"Financial transaction with ID {financialTransactionId} not found."));
                _logger.LogDebug("FinancialTransaction {FinancialTransactionId} not found.", financialTransactionId);
                return;
            }

            var user = context.HttpContext.User;
            _logger.LogDebug(
                "Financial transaction: {@FinancialTransaction}, userId: {UserId}, policy:{Policy}",
                financialTransaction,
                user.GetUserId(),
                Policies.TransactionAccess);
            var authResult = await _authorizationService.AuthorizeAsync(user, financialTransaction, Policies.TransactionAccess);

            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransactions.Common.NoAccess,
                    "Access denied to financial transaction {FinancialTransactionId}",
                    financialTransactionId);

                context.Result = new ObjectResult(ApiResponse.Forbidden<object>("Forbidden access to financial transaction."))
                {
                    StatusCode = 403,
                };
                return;
            }

            await next();
        }
    }

    public class FinancialTransactionAuthorizationAttribute : TypeFilterAttribute
    {
        public FinancialTransactionAuthorizationAttribute()
            : base(typeof(FinancialTransactionAuthorizationFilter))
        {
        }
    }
}
