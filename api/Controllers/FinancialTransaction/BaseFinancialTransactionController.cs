using api.Dtos.FinancialTransaction;
using api.Filters;
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
    public abstract class BaseFinancialTransactionController : ControllerBase
    {
        protected readonly IFinancialTransactionService _financialTransactionService;
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The service used to manage transactions.</param>
        /// <param name="logger">The logger for recording transaction events.</param>
        /// <param name="authorizationService">The service used to authorize transaction access.</param>
        public BaseFinancialTransactionController(
            IFinancialTransactionService transactionService,
            ILogger logger,
            IAuthorizationService authorizationService)
        {
            _financialTransactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a transaction by its identifier.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the transaction if found.
        /// </returns>
        [HttpGet("{id:int}")]
        [FinancialTransactionAuthorization]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> GetById([FromRoute] int id)
        {
            _logger.LogDebug("id is {Id}", id);
            var result = await _financialTransactionService.GetByIdAsync(id);

            return ApiResponse.Success(result!);
        }

        /// <summary>
        /// Updates an existing transaction.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <param name="dto">The updated transaction data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the updated transaction.
        /// </returns>
        [HttpPut("{id:int}")]
        [FinancialTransactionAuthorization]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseFinancialTransactionInputDto dto)
        {
            var result = await _financialTransactionService.UpdateAsync(id, dto);

            return ApiResponse.Success(result!);
        }

        /// <summary>
        /// Deletes a transaction by its identifier.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the deletion was successful.
        /// </returns>
        [HttpDelete("{id:int}")]
        [FinancialTransactionAuthorization]
        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            var result = await _financialTransactionService.DeleteAsync(id);
            return ApiResponse.Success(result!);
        }
    }
}
