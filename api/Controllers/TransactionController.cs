using System.Linq.Expressions;
using api.Data;
using api.Dtos;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        public TransactionController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
        /// <summary>
        /// Get a report grouped by category.
        /// </summary>
        /// <param name="dateRange">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<GroupedReportDto>>))]
        [HttpGet("reportByCategory")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReportByCategory(
     [FromQuery] ReportQueryObject? dateRange)
        {
            var report = await _transactionRepository.GetReportByCategoryAsync(dateRange);
            return ApiResponse.Success(report);
        }
        /// <summary>
        /// Get a report grouped by date(month and year).
        /// </summary>
        /// <param name="dateRange">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<GroupedReportDto>>))]
        [HttpGet("reportByDate")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReportByDate(
          [FromQuery] ReportQueryObject? dateRange)
        {
            var report = await _transactionRepository.GetReportByDateAsync(dateRange);
            return ApiResponse.Success(report);
        }
        /// <summary>
        /// Get a report grouped by both category and date(month and year).
        /// </summary>
        /// <param name="dateRange">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<GroupedReportDto>>))]
        [HttpGet("reportCategoryAndDate")]
        public async Task<ApiResponse<List<GroupedReportDto>>> GetReportByCategoryAndDate(
            [FromQuery] ReportQueryObject? dateRange)
        {
            var report = await _transactionRepository.GetReportByCategoryAndDateAsync(dateRange);
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
            var transactions = await _transactionRepository.GetAllAsync();
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
            var transaction = await _transactionRepository.GetByIdAsync(id);
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

            FinancialTransaction transaction = new FinancialTransaction
            {
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Comment = transactionDto.Comment
            };
            var createdTransaction = await _transactionRepository.CreateAsync(transaction);
            return ApiResponse.Success(createdTransaction);
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
            if (await _transactionRepository.GetByIdAsync(id) == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
            }
            FinancialTransaction transaction = new FinancialTransaction
            {
                Id = id,
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Comment = transactionDto.Comment
            };
            await _transactionRepository.UpdateAsync(id, transaction);
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
        public async Task<ApiResponse<FinancialTransaction>> Delete([FromRoute] int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
            }
            await _transactionRepository.DeleteAsync(id);
            return ApiResponse.Success(transaction);
        }
       
    }
}