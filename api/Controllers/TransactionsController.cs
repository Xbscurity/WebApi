using api.Dtos;
using api.Enums;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ITimeProvider _timeProvider;
        public TransactionsController(ITransactionsRepository transactionRepository, ITimeProvider timeProvider)
        {
            _transactionsRepository = transactionRepository;
            _timeProvider = timeProvider;
        }
        /// <summary>
        /// Get a report by specified group
        /// </summary>
        /// <param name="dateRange">The date range for filtering the report.</param>
        /// <param name="reportType">
        /// The type of report to generate: 
        /// 0 - "Category" - Report grouped by category.
        /// 1 - "Date" - Report grouped by date.  
        /// 2 - "CategoryAndDate" - Report grouped by both category and date.
        /// </param>
        /// <returns>A list of grouped reports.</returns>
        [HttpGet("report")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReport(
    [FromQuery] ReportQueryObject? dateRange,
    [FromQuery] ReportType reportType = ReportType.Category)    
        {
            List<GroupedReportDto> report = reportType switch
            {
                ReportType.Category => await _transactionsRepository.GetReportByCategoryAsync(dateRange),
                ReportType.Date => await _transactionsRepository.GetReportByDateAsync(dateRange),
                ReportType.CategoryAndDate => await _transactionsRepository.GetReportByCategoryAndDateAsync(dateRange)
            };
            return ApiResponse.Success(report);
        }
    /// <summary>
    /// Get all transactions.
    /// </summary>
    /// <returns>A list of all transactions</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialTransaction>))]
    [HttpGet]
    public async Task<ApiResponse<List<FinancialTransaction>>> GetAll()
    {
        var transactions = await _transactionsRepository.GetAllAsync();
        return ApiResponse.Success(transactions);
    }
    /// <summary>
    /// Get a specific transaction by its ID.
    /// </summary>
    /// <param name="id">The ID of the transaction to get.</param>
    /// <returns>The transaction with the specified ID.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialTransaction>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<FinancialTransaction>> GetById([FromRoute] int id)
    {
        var transaction = await _transactionsRepository.GetByIdAsync(id);
        if (transaction == null)
        {
            return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
        }
        return ApiResponse.Success(transaction);
    }
    /// <summary>
    /// CreateAsync a new financial transaction.
    /// </summary>
    /// <param name="transactionDto">The data for the new transaction.</param>
    /// <returns>The created transaction with its details</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<FinancialTransaction>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
    [HttpPost]
    public async Task<ApiResponse<FinancialTransaction>> Create([FromBody] FinancialTransactionDto transactionDto)
    {

        FinancialTransaction transaction = new FinancialTransaction(_timeProvider)
        {
            CategoryId = transactionDto.CategoryId,
            Amount = transactionDto.Amount,
            Comment = transactionDto.Comment
        };
        await _transactionsRepository.CreateAsync(transaction);
        return ApiResponse.Success(transaction);
    }
    /// <summary>
    /// UpdateAsync an existing financial transaction.
    /// </summary>
    /// <param name="id">The Id of the transaction to update.</param>
    /// <param name="transactionDto">The updated transaction data.</param>
    /// <returns>No content if the update is successful.</returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
    [HttpPut("{id:int}")]
    public async Task<ApiResponse<FinancialTransaction>> Update([FromRoute] int id, [FromBody] FinancialTransactionDto transactionDto)
    {
        if (await _transactionsRepository.GetByIdAsync(id) == null)
        {
            return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
        }
        FinancialTransaction transaction = new FinancialTransaction(_timeProvider)
        {
            Id = id,
            CategoryId = transactionDto.CategoryId,
            Amount = transactionDto.Amount,
            Comment = transactionDto.Comment
        };
        await _transactionsRepository.UpdateAsync(transaction);
        return ApiResponse.Success(transaction);
    }
    /// <summary>
    /// DeleteAsync an existing financial transaction.
    /// </summary>
    /// <param name="id">The ID of the transaction to delete.</param>
    /// <returns>No content if the delete is successful.</returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
    {
        var transaction = await _transactionsRepository.GetByIdAsync(id);
        if (transaction == null)
        {
            return ApiResponse.NotFound<bool>("Transaction not found");
        }
        await _transactionsRepository.DeleteAsync(transaction);
        return ApiResponse.Success(true);
    }
        [HttpGet("time")]
        public async Task<ApiResponse<DateTimeOffset>> GetTime()
        {

            return ApiResponse.Success(_timeProvider.UtcNow);
        }

}
}
