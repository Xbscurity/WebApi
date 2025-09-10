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

        public UserTransactionController(
            ITransactionService transactionService,
            TransactionSortValidator sortValidator,
            ILogger<UserTransactionController> logger,
            IAuthorizationService authorizationService)
            : base(transactionService, logger, authorizationService)
        {
            _sortValidator = sortValidator;
        }

        [HttpGet("report")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReport(
    [FromQuery] ReportQueryObject? queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(LoggingEvents.Transactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<GroupedReportDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }
            var userId = User.GetUserId();
            var report = await _transactionService.GetReportAsync(userId!, queryObject);
            _logger.LogInformation(
                LoggingEvents.Transactions.User.Report,
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
                _logger.LogWarning(LoggingEvents.Transactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }
            var userId = User.GetUserId();
            var transactions = await _transactionService.GetAllForUserAsync(userId!, queryObject);
            _logger.LogInformation(
                LoggingEvents.Transactions.User.GetAll,
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
            var userId = User.GetUserId();
            var result = await _transactionService.CreateForUserAsync(userId!, transactionDto);
            _logger.LogInformation(LoggingEvents.Transactions.User.Created, "Created new transaction {transactionId}", result.Id);
            return ApiResponse.Success(result);
        }
    }
}
