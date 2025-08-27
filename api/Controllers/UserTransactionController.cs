using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Extensions;
using api.QueryObjects;
using api.Responses;
using api.Services.Transaction;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.UserNotBanned)]
    [ApiController]
    [Route("api/user/transactions")]
    public class UserTransactionController : BaseTransactionController
    {
        private readonly TransactionSortValidator _sortValidator;

        public UserTransactionController(ITransactionService transactionService, ILogger<UserTransactionController> logger)
            : base(transactionService, logger)
        {
        }

        /// <summary>
        /// Get a report by specified group
        /// </summary>
        /// <param name="dateRange">The date range for filtering the report.</param>
        /// <param name="reportType">
        /// The type of report to generate: 
        /// 1 - "ByCategory" - Report grouped by category.
        /// 2 - "ByDate" - Report grouped by date.  
        /// 3 - "ByCategoryAndDate" - Report grouped by both category and date.
        /// </param>
        /// <returns>A list of grouped reports.</returns>
        ///
        [HttpGet("report")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReport(
    [FromQuery] ReportQueryObject? queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(_sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<GroupedReportDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var report = await _transactionService.GetReportAsync(User.ToCurrentUser(), queryObject);
            _logger.LogInformation(
                "Returning {Count} transactions. Strategy Key = {StrategyKey}, Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                report.Data.Count,
                queryObject.Key,
                report.Pagination.PageNumber,
                report.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(report.Data, report.Pagination);
        }

        [HttpGet]
        public async Task<ApiResponse<List<BaseFinancialTransactionOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(_sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var transactions = await _transactionService.GetAllForUserAsync(User.ToCurrentUser(), queryObject);
            _logger.LogInformation(
                "Returning {Count} transactions. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                transactions.Data.Count,
                transactions.Pagination.PageNumber,
                transactions.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Create([FromBody] BaseFinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionService.CreateForUserAsync(User.ToCurrentUser(), transactionDto);
            _logger.LogInformation("Created new transaction {transactionId}", result.Id);
            return ApiResponse.Success(result);
        }
    }
}
