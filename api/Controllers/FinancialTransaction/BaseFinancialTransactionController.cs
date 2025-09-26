using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Filters;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.FinancialTransaction
{
    /// <summary>
    /// Provides a base controller for managing financial transactions.
    /// Contains shared logic for retrieving, updating, and deleting transactions.
    /// </summary>
    public abstract class BaseFinancialTransactionController : ControllerBase
    {
        /// <summary>
        /// The service used to manage transactions.
        /// </summary>
        private readonly IFinancialTransactionService _financialTransactionService;

        /// <summary>
        /// The logger for recording transaction events.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The service used to manage transactions.</param>
        /// <param name="logger">The logger for recording transaction events.</param>
        public BaseFinancialTransactionController(
            IFinancialTransactionService transactionService,
            ILogger logger)
        {
            _financialTransactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the service for managing financial transactions.
        /// </summary>
        protected IFinancialTransactionService FinancialTransactionService => _financialTransactionService;

        /// <summary>
        /// Gets the logger for recording category-related events.
        /// </summary>
        protected ILogger Logger => _logger;

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
            var result = await FinancialTransactionService.GetByIdAsync(id);
            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.GetById,
                "Financial transaction with ID {FinancialTransactionId} retrieved.",
                id);
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
            var result = await FinancialTransactionService.UpdateAsync(id, dto);

            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.Updated,
                "Financial transaction with ID {FinancialTransactionId} updated.",
                id);
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
            var result = await FinancialTransactionService.DeleteAsync(id);

            Logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.Deleted,
                "Financial transaction with ID {FinancialTransactionId} deleted.",
                id);
            return ApiResponse.Success(result!);
        }
    }
}
