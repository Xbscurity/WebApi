using api.Constants;
using api.Dtos.FinancialTransactions;
using api.Extensions;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseTransactionController : ControllerBase
    {
        protected readonly ITransactionService _transactionService;
        protected readonly ILogger _logger;

        public BaseTransactionController(ITransactionService transactionService, ILogger logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> GetById([FromRoute] int id)
        {
            var transaction = await _transactionService.GetByIdAsync(User.ToCurrentUser(), id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<BaseFinancialTransactionOutputDto>("Transaction not found");
            }

            _logger.LogInformation(LoggingEvents.Transactions.Common.GetById, "Returning transaction. TransactionId {TransactionId}", id);
            return ApiResponse.Success(transaction);
        }

        [HttpPut("{id:int}")]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseFinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionService.UpdateAsync(User.ToCurrentUser(), id, transactionDto);
            if (result is null)
            {
                return ApiResponse.NotFound<BaseFinancialTransactionOutputDto>("Transaction not found");
            }

            _logger.LogInformation(
                LoggingEvents.Transactions.Common.Updated,
                "Transaction updated successfully. TransactionId: {TransactionId}",
                id);
            return ApiResponse.Success(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            bool result = await _transactionService.DeleteAsync(User.ToCurrentUser(), id);
            if (result is false)
            {
                return ApiResponse.NotFound<bool>("Transaction not found");
            }

            _logger.LogInformation(
                LoggingEvents.Transactions.Common.Deleted,
                "Transaction deleted successfully. TransactionId: {TransactionId}",
                id);
            return ApiResponse.Success(result);
        }
    }
}
