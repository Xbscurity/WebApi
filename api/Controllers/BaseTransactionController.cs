using api.Constants;
using api.Dtos.FinancialTransactions;
using api.Extensions;
using api.Models;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseTransactionController : ControllerBase
    {
        protected readonly ITransactionService _transactionService;
        protected readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;

        public BaseTransactionController(
            ITransactionService transactionService,
            ILogger logger,
            IAuthorizationService authorizationService)
        {
            _transactionService = transactionService;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        [HttpGet("{id:int}")]
        public Task<ApiResponse<BaseFinancialTransactionOutputDto>> GetById([FromRoute] int id) =>
    HandleTransactionAsync(id, t => Task.FromResult(t.ToOutputDto())!);

        [HttpPut("{id:int}")]
        public Task<ApiResponse<BaseFinancialTransactionOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseFinancialTransactionInputDto dto) =>
            HandleTransactionAsync(id, t => _transactionService.UpdateAsync(id, dto));

        [HttpDelete("{id:int}")]
        public Task<ApiResponse<bool>> Delete([FromRoute] int id) =>
            HandleTransactionAsync(id, t => _transactionService.DeleteAsync(id));


        private async Task<ApiResponse<T>> HandleTransactionAsync<T>(
    int id, Func<FinancialTransaction, Task<T?>> action)
        {
            var transaction = await _transactionService.GetByIdRawAsync(id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<T>("Transaction not found");
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, transaction, Policies.TransactionAccess);
            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    LoggingEvents.Transactions.Common.NoAccess,
                    "Access denied to transaction {TransactionId}",
                    id);

                return ApiResponse.NotFound<T>("Transaction not found");
            }

            var result = await action(transaction);
            return ApiResponse.Success(result!);
        }
    }
}
