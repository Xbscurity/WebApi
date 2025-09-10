using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.QueryObjects;
using api.Responses;
using api.Services.Transaction;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/admin/transactions")]
    public class AdminTransactionController : BaseTransactionController
    {
        private readonly TransactionSortValidator _sortValidator;

        public AdminTransactionController(
            ITransactionService transactionService,
            TransactionSortValidator sortValidator,
            ILogger<AdminTransactionController> logger,
            IAuthorizationService authorizationService)
            : base(transactionService, logger, authorizationService)
        {
            _sortValidator = sortValidator;
        }

        [HttpGet]
        public async Task<ApiResponse<List<BaseFinancialTransactionOutputDto>>> GetAll(
            [FromQuery] PaginationQueryObject queryObject,
            [FromQuery] string? userId = null)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(LoggingEvents.Transactions.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>(
                    _sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var transactions = await _transactionService.GetAllForAdminAsync(queryObject, userId);
            _logger.LogInformation(
                LoggingEvents.Transactions.Admin.GetAll,
                "Returning {Count} transactions. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}, userId = {userId}",
                transactions.Data.Count,
                transactions.Pagination.PageNumber,
                transactions.Pagination.PageSize,
                queryObject.SortBy,
                userId);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Create(
            [FromBody] AdminFinancialTransactionCreateInputDto transactionDto)
        {
            var result = await _transactionService.CreateForAdminAsync(transactionDto.AppUserId!, transactionDto);
            _logger.LogInformation(
                LoggingEvents.Categories.Admin.Created,
                "Created new transaction {transactionId} for user {UserId}",
                result.Id,
                result.AppUserId);
            return ApiResponse.Success(result);
        }
    }
}
