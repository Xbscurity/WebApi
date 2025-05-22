using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.Models;
using api.QueryObjects;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionsService;

        public TransactionController(ITransactionService transactionsService)
        {
            _transactionsService = transactionsService;
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
        [HttpGet("report")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReport(
    [FromQuery] ReportQueryObject? queryObject)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
              {
                  "id", "category", "amount", "date",
              };
            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<GroupedReportDto>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }
            var report = await _transactionsService.GetReportAsync(queryObject);
            return ApiResponse.Success(report.Data, report.Pagination);
        }

        /// <summary>
        /// Get all transactions.
        /// </summary>
        /// <returns>A list of all transactions</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpGet]
        public async Task<ApiResponse<List<FinancialTransactionOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
              {
                  "id", "category", "amount", "date",
              };
            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<FinancialTransactionOutputDto>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }

            var transactions = await _transactionsService.GetAllAsync(queryObject);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        /// <summary>
        /// Get a specific transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction to get.</param>
        /// <returns>The transaction with the specified ID.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialTransaction>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpGet("{id:int}")]
        public async Task<ApiResponse<FinancialTransactionOutputDto>> GetById([FromRoute] int id)
        {
            var transaction = await _transactionsService.GetByIdAsync(id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<FinancialTransactionOutputDto>("Transaction not found");
            }

            return ApiResponse.Success(transaction);
        }

        /// <summary>
        /// Create a new financial transaction.
        /// </summary>
        /// <param name="transactionDto">The data for the new transaction.</param>
        /// <returns>The created transaction with its details</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<FinancialTransaction>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpPost]
        public async Task<ApiResponse<FinancialTransactionOutputDto>> Create([FromBody] FinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionsService.CreateAsync(transactionDto);
            return ApiResponse.Success(result);
        }

        /// <summary>
        /// Update an existing financial transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction to update.</param>
        /// <param name="transactionDto">The updated transaction data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpPut("{id:int}")]
        public async Task<ApiResponse<FinancialTransactionOutputDto>> Update([FromRoute] int id, [FromBody] FinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionsService.UpdateAsync(id, transactionDto);
            if (result is null)
            {
                return ApiResponse.NotFound<FinancialTransactionOutputDto>("Transaction not found");
            }

            return ApiResponse.Success(result);
        }

        /// <summary>
        /// Delete an existing financial transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpDelete("{id:int}")]
        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            bool result = await _transactionsService.DeleteAsync(id);
            if (result is false)
            {
                return ApiResponse.NotFound<bool>("Transaction not found");
            }

            return ApiResponse.Success(result);
        }

    }
}
