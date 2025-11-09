using api.Constants;
using api.Dtos.FinancialTransaction;
using api.QueryObjects;
using api.Responses;
using api.Services.Transaction;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.FinancialTransaction
{
    /// <summary>
    /// Provides API endpoints for administrators to manage transactions across all users.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/admin/transactions")]
    public class AdminFinancialTransactionController : BaseFinancialTransactionController
    {
        private readonly FinancialTransactionSortValidator _sortValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The service used to manage transactions.</param>
        /// <param name="sortValidator">Validates sorting fields for transaction queries.</param>
        /// <param name="logger">The logger for admin-specific transaction operations.</param>
        public AdminFinancialTransactionController(
            IFinancialTransactionService transactionService,
            FinancialTransactionSortValidator sortValidator,
            ILogger<AdminFinancialTransactionController> logger)
            : base(transactionService, logger)
        {
            _sortValidator = sortValidator;
        }

        /// <summary>
        /// Retrieves all transactions with optional filtering by user.
        /// </summary>
        /// <param name="queryObject">The pagination and sorting query parameters.</param>
        /// <param name="userId">Optional user ID to filter transactions by owner.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of transactions.
        /// </returns>
        [HttpGet]
        public async Task<ApiResponse<List<BaseFinancialTransactionOutputDto>>> GetAll(
            [FromQuery] PaginationQueryObject queryObject,
            [FromQuery] string? userId = null)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                Logger.LogWarning(LoggingEvents.FinancialTransactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>(
                    _sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var transactions = await FinancialTransactionService.GetAllForAdminAsync(queryObject, userId);
            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.Admin.GetAll,
                "Returning {Count} transactions. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}, userId = {userId}",
                transactions.Data.Count,
                transactions.Pagination.PageNumber,
                transactions.Pagination.PageSize,
                queryObject.SortBy,
                userId);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        /// <summary>
        /// Creates a new transaction on behalf of a user.
        /// </summary>
        /// <param name="transactionDto">The transaction creation data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the newly created transaction and location header to it.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BaseFinancialTransactionMinimalOutputDto>>> Create(
            [FromBody] AdminFinancialTransactionInputDto transactionDto)
        {
            var result = await FinancialTransactionService.CreateForAdminAsync(transactionDto);

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.Id },
                value: ApiResponse.Success(result));
        }
    }
}
