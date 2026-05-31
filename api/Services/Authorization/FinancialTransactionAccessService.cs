using api.Constants;
using api.Models;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace api.Services.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="IFinancialTransactionAccessService"/>.
    /// </summary>
    /// <remarks>
    /// This service validates whether the current authenticated user
    /// is permitted to access a specific financial transaction
    /// using the configured authorization policies.
    /// </remarks>
    public class FinancialTransactionAccessService : IFinancialTransactionAccessService
    {
        private readonly IHttpContextAccessor _context;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="FinancialTransactionAccessService"/> class.
        /// </summary>
        /// <param name="context">
        /// The HTTP context accessor used to retrieve the current user context.
        /// </param>
        /// <param name="authorizationService">
        /// The authorization service used to evaluate access policies.
        /// </param>
        public FinancialTransactionAccessService(
            IHttpContextAccessor context,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<Success>> CanAccessCheckAsync(FinancialTransaction financialTransaction)
        {
            if (_context.HttpContext == null)
            {
                return Errors.FT.AccessDenied(financialTransaction.Id);
            }

            var authResult = await _authorizationService.AuthorizeAsync(
                _context.HttpContext.User,
                financialTransaction,
                Policies.FinancialTransactionAccess);

            if (!authResult.Succeeded)
            {
                return Errors.FT.AccessDenied(financialTransaction.Id);
            }

            return Result.Success;
        }
    }
}
