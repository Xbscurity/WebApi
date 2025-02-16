using System.Linq.Expressions;
using api.Data;
using api.Dtos;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
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
            var groupedTransactions = await GetGroupedTransactions(
                dateRange,
                t => t.Category == null ? "No category" : t.Category.Name,
                key => new ReportKey { Category = key }
            );
            return ApiResponse.Success(groupedTransactions);
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
            var groupedTransactions = await GetGroupedTransactions(
               dateRange,
               t => new { t.Date.Month, t.Date.Year },
               key => new ReportKey { Month = key.Month, Year = key.Year }
           );
            return ApiResponse.Success(groupedTransactions);
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
            var groupedTransactions = await GetGroupedTransactions(
                dateRange,
                t => new { Category = t.Category.Name ?? "No category", t.Date.Month, t.Date.Year },
                key => new ReportKey { Category = key.Category, Month = key.Month, Year = key.Year }
            );
            return ApiResponse.Success(groupedTransactions);
        }
        /// <summary>
        /// Get all transactions.
        /// </summary>
        /// <returns>A list of all transactions</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpGet]
        public async Task<ApiResponse<List<FinancialTransaction>>> GetAll()
        {
            throw new Exception("Custom error idk");
            var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category).ToListAsync();
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
            var transaction = await _context.Transactions.FindAsync(id);
           // throw new Exception("lol my bad");
            if (transaction == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
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
        public async Task<ApiResponse<FinancialTransaction>> Create([FromBody] FinancialTransactionDto transactionDto)
        {
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Category not found");
            }
            FinancialTransaction transaction = new FinancialTransaction
            {
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Comment = transactionDto.Comment
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return ApiResponse.Success(transaction);
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
        public async Task<ApiResponse<FinancialTransaction>> Update([FromRoute] int id, [FromBody] FinancialTransactionDto transactionDto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
            }
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Category not found");
            }
            transaction.Amount = transactionDto.Amount;
            transaction.CategoryId = transactionDto.CategoryId;
            transaction.Comment = transactionDto.Comment;
            await _context.SaveChangesAsync();
            return ApiResponse.Success(transaction);
        }
        /// <summary>
        /// Delete an existing financial transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<FinancialTransaction>))]
        [HttpDelete("{id}")]
        public async Task<ApiResponse<FinancialTransaction>> Delete([FromRoute] int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<FinancialTransaction>("Transaction not found");
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return ApiResponse.Success(transaction);
        }
        private IQueryable<FinancialTransaction> GetFilteredTransactions(ReportQueryObject? dateRange)
        {
            var transactions = _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .AsQueryable();

            if (dateRange?.StartDate != null)
            {
                transactions = transactions.Where(t => t.Date >= dateRange.StartDate);
            }
            if (dateRange?.EndDate != null)
            {
                transactions = transactions.Where(t => t.Date <= dateRange.EndDate);
            }

            return transactions;
        }
        private async Task<List<GroupedReportDto>> GetGroupedTransactions<TGroupKey>(
            ReportQueryObject? dateRange,
            Expression<Func<FinancialTransaction, TGroupKey>> groupBySelector,
            Func<TGroupKey, ReportKey> keySelector)
        {
            var transactions = GetFilteredTransactions(dateRange);

            return await transactions
                .GroupBy(groupBySelector)
                .Select(group => new GroupedReportDto
                {
                    Key = keySelector(group.Key),
                    Transactions = group.Select(transaction => new ReportTransactionDto
                    {
                        Category = transaction.Category.Name ?? "No category",
                        Date = transaction.Date,
                        Amount = transaction.Amount,
                        Comment = transaction.Comment
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}