using api.Constants;
using api.Extensions;
using api.Repositories.Interfaces;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    /// <summary>
    /// An asynchronous action filter that performs authorization for financial transaction-related actions.
    /// It checks if the authenticated user has the necessary permissions to access a specific financial transaction.
    /// </summary>
    public class FinancialTransactionAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<FinancialTransactionAuthorizationFilter> _logger;
        private readonly IFinancialTransactionRepository _financialTransactionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAuthorizationFilter"/> class.
        /// </summary>
        /// <param name="authorizationService">The service for performing authorization checks.</param>
        /// <param name="logger">The logger for this filter.</param>
        /// <param name="financialTransactionRepository">The repository used to manage transactions.</param>
        public FinancialTransactionAuthorizationFilter(
           IAuthorizationService authorizationService,
           ILogger<FinancialTransactionAuthorizationFilter> logger,
           IFinancialTransactionRepository financialTransactionRepository)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _financialTransactionRepository = financialTransactionRepository;
        }

        /// <summary>
        /// Called before and after the action is executed. It validates the financial transaction ID, retrieves the transaction,
        /// and authorizes the user against the specified policy.
        /// </summary>
        /// <param name="context">The context for action execution.</param>
        /// <param name="next">The delegate to call to proceed to the next action filter or the action itself.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execution.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idObj) || idObj is not int financialTransactionId)
            {
                context.Result = new BadRequestObjectResult(ApiResponse.BadRequest<object>());
                return;
            }

            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(financialTransactionId);

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

    /// <summary>
    /// An attribute to apply the <see cref="FinancialTransactionAuthorizationFilter"/> to an action method.
    /// This provides a declarative way to require authorization for financial transaction-related actions.
    /// </summary>
    public class FinancialTransactionAuthorizationAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAuthorizationAttribute"/> class.
        /// </summary>
        public FinancialTransactionAuthorizationAttribute()
            : base(typeof(FinancialTransactionAuthorizationFilter))
        {
        }
    }
}
