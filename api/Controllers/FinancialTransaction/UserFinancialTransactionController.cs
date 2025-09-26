using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Extensions;
using api.QueryObjects;
using api.Responses;
using api.Services.Transaction;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.FinancialTransaction
{
    /// <summary>
    /// Provides API endpoints for users to manage their own transactions.
    /// </summary>
    [Authorize(Policy = Policies.UserNotBanned)]
    [ApiController]
    [Route("api/user/transactions")]
    public class UserFinancialTransactionController : BaseFinancialTransactionController
    {
        private readonly FinancialTransactionSortValidator _sortValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The service used to manage transactions.</param>
        /// <param name="sortValidator">Validates sorting fields for transaction queries.</param>
        /// <param name="logger">The logger for user-specific transaction operations.</param>
        public UserFinancialTransactionController(
            IFinancialTransactionService transactionService,
            FinancialTransactionSortValidator sortValidator,
            ILogger<UserFinancialTransactionController> logger)
            : base(transactionService, logger)
        {
            _sortValidator = sortValidator;
        }

        /// <summary>
        /// Retrieves a grouped report of transactions for the authenticated user.
        /// </summary>
        /// <param name="queryObject">The report query parameters, including pagination and grouping strategy.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the grouped transaction report.
        /// </returns>
        [HttpGet("report")]
        public async Task<ApiResponse<List<GroupedReportOutputDto>>> GetReport(
    [FromQuery] ReportQueryObject? queryObject)
        {
            if (!_sortValidator.IsValid(queryObject!.SortBy))
            {
                Logger.LogWarning(LoggingEvents.FinancialTransactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<GroupedReportOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var userId = User.GetUserId();
            var report = await FinancialTransactionService.GetReportAsync(userId!, queryObject);
            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.User.Report,
                "Returning {Count} transactions. Strategy Key = {StrategyKey}, Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                report.Data.Count,
                queryObject.Key,
                report.Pagination.PageNumber,
                report.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(report.Data, report.Pagination);
        }

        /// <summary>
        /// Retrieves all transactions for the authenticated user.
        /// </summary>
        /// <param name="queryObject">The pagination and sorting query parameters.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of transactions.
        /// </returns>
        [HttpGet]
        public async Task<ApiResponse<List<BaseFinancialTransactionOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                Logger.LogWarning(LoggingEvents.FinancialTransactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var userId = User.GetUserId();
            var transactions = await FinancialTransactionService.GetAllForUserAsync(userId!, queryObject);
            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.User.GetAll,
                "Returning {Count} transactions. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                transactions.Data.Count,
                transactions.Pagination.PageNumber,
                transactions.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        /// <summary>
        /// Creates a new transaction for the authenticated user.
        /// </summary>
        /// <param name="transactionDto">The transaction creation data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the newly created transaction.
        /// </returns>
        [HttpPost]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Create(
            [FromBody] BaseFinancialTransactionInputDto transactionDto)
        {
            var userId = User.GetUserId();
            var result = await FinancialTransactionService.CreateForUserAsync(userId!, transactionDto);
            if (result == null)
            {
                Logger.LogInformation(
                                LoggingEvents.Categories.Common.GetById,
                                "Category {CategoryId} not found",
                                transactionDto.CategoryId);
                return ApiResponse.NotFound<BaseFinancialTransactionOutputDto>($"Category {transactionDto.CategoryId} not found");
            }

            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.User.Created,
                "Created new transaction {transactionId}",
                result.Id);
            return ApiResponse.Success(result);
        }
    }
}
