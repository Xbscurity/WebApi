using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Extensions;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.FinancialTransaction
{
    /// <summary>
    /// Provides a base controller for managing financial transactions.
    /// Contains shared logic for retrieving, updating, and deleting transactions.
    /// </summary>
    public abstract class BaseTransactionController : ControllerBase
    {
        protected readonly ITransactionService _transactionService;
        protected readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The service used to manage transactions.</param>
        /// <param name="logger">The logger for recording transaction events.</param>
        /// <param name="authorizationService">The service used to authorize transaction access.</param>
        public BaseTransactionController(
            ITransactionService transactionService,
            ILogger logger,
            IAuthorizationService authorizationService)
        {
            _transactionService = transactionService;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Retrieves a transaction by its identifier.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the transaction if found.
        /// </returns>
        [HttpGet("{id:int}")]
        public Task<ApiResponse<BaseFinancialTransactionOutputDto>> GetById([FromRoute] int id) =>
    HandleTransactionAsync(id, t => Task.FromResult(t.ToOutputDto())!);

        /// <summary>
        /// Updates an existing transaction.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <param name="dto">The updated transaction data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the updated transaction.
        /// </returns>
        [HttpPut("{id:int}")]
        public Task<ApiResponse<BaseFinancialTransactionOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseFinancialTransactionInputDto dto) =>
            HandleTransactionAsync(id, t => _transactionService.UpdateAsync(id, dto));

        /// <summary>
        /// Deletes a transaction by its identifier.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the deletion was successful.
        /// </returns>
        [HttpDelete("{id:int}")]
        public Task<ApiResponse<bool>> Delete([FromRoute] int id) =>
            HandleTransactionAsync(id, t => _transactionService.DeleteAsync(id));


        private async Task<ApiResponse<T>> HandleTransactionAsync<T>(
    int id, Func<Models.FinancialTransaction, Task<T?>> action)
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
