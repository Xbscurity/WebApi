using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using api.Data;
using api.Dtos;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        /// <param name="query">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<List<GroupedReportDto>>))]
        [HttpGet("reportByCategory")]
        public async Task<ActionResult<Response<List<GroupedReportDto>>>> GetReportByCategory(
     [FromQuery] ReportQueryObject? query)
        {
            var groupedTransactions = await GetGroupedTransactions(
                query,
                t => t.Category != null ? t.Category.Name : "No category",
                key => new ReportKey { Category = key }
            );
            return Ok(Response<List<GroupedReportDto>>.SuccessResponse(groupedTransactions));
        }
        /// <summary>
        /// Get a report grouped by date(month and year).
        /// </summary>
        /// <param name="query">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<List<GroupedReportDto>>))]
        [HttpGet("reportByDate")]
        public async Task<ActionResult<Response<List<GroupedReportDto>>>> GetReportByDate(
          [FromQuery] ReportQueryObject? query)
        {
            var groupedTransactions = await GetGroupedTransactions(
               query,
               t => new { t.Date.Month, t.Date.Year },
               key => new ReportKey { Month = key.Month, Year = key.Year }
           );
            return Ok(Response<List<GroupedReportDto>>.SuccessResponse(groupedTransactions));
        }
        /// <summary>
        /// Get a report grouped by both category and date(month and year).
        /// </summary>
        /// <param name="query">The report query parameters(data range).</param>
        /// <returns>A grouped report response containing the data.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<List<GroupedReportDto>>))]
        [HttpGet("reportCategoryAndDate")]
        public async Task<ActionResult<Response<List<GroupedReportDto>>>> GetReportByCategoryAndDate(
            [FromQuery] ReportQueryObject? query)
        {
            var groupedTransactions = await GetGroupedTransactions(
                query,
                t => new { Category = t.Category.Name ?? "No category", t.Date.Month, t.Date.Year },
                key => new ReportKey { Category = key.Category, Month = key.Month, Year = key.Year }
            );
            return Ok(Response<List<GroupedReportDto>>.SuccessResponse(groupedTransactions));
        }
        /// <summary>
        /// Get all transactions.
        /// </summary>
        /// <returns>A list of all transactions</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<FinancialTransaction>))]
        [HttpGet]
        public async Task<ActionResult<Response<List<FinancialTransaction>>>> GetAll()
        {
            var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category).ToListAsync();
            return Ok(Response<List<FinancialTransaction>>.SuccessResponse(transactions));
        }
        /// <summary>
        /// Get a specific transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction to get.</param>
        /// <returns>The transaction with the specified ID.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<FinancialTransaction>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<FinancialTransaction>))]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Response<FinancialTransaction>>> GetById([FromRoute] int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(Response<FinancialTransaction>.NotFoundResponse("Transaction not found"));
            }
            return Ok(Response<FinancialTransaction>.SuccessResponse(transaction));
        }
        /// <summary>
        /// Create a new financial transaction.
        /// </summary>
        /// <param name="transactionDto">The data for the new transaction.</param>
        /// <returns>The created transaction with its details</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Response<FinancialTransaction>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<FinancialTransaction>))]
        [HttpPost]
        public async Task<ActionResult<Response<FinancialTransaction>>> Create([FromBody] FinancialTransactionDto transactionDto)
        {
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return NotFound(Response<FinancialTransaction>.NotFoundResponse("Category not found"));
            }
            FinancialTransaction transaction = new FinancialTransaction
            {
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Comment = transactionDto.Comment
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, Response<FinancialTransaction>.SuccessResponse(transaction));
        }
        /// <summary>
        /// Update an existing financial transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction to update.</param>
        /// <param name="transactionDto">The updated transaction data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<FinancialTransaction>))]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Response<FinancialTransaction>>> Update([FromRoute] int id, [FromBody] FinancialTransactionDto transactionDto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(Response<FinancialTransaction>.NotFoundResponse("Transaction not found"));
            }
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return NotFound(Response<FinancialTransaction>.NotFoundResponse("Category not found"));
            }
            transaction.Amount = transactionDto.Amount;
            transaction.CategoryId = transactionDto.CategoryId;
            transaction.Comment = transactionDto.Comment;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Delete an existing financial transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<FinancialTransaction>))]
        [HttpDelete("{id}")]
        public async Task<ActionResult<FinancialTransaction>> Delete([FromRoute] int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(Response<FinancialTransaction>.NotFoundResponse("Transaction not found"));
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private IQueryable<FinancialTransaction> GetFilteredTransactions(ReportQueryObject? query)
        {
            var transactions = _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .AsQueryable();

            if (query?.startDate != null)
            {
                transactions = transactions.Where(t => t.Date >= query.startDate);
            }
            if (query?.endDate != null)
            {
                transactions = transactions.Where(t => t.Date <= query.endDate);
            }

            return transactions;
        }
        private async Task<List<GroupedReportDto>> GetGroupedTransactions<TGroupKey>(
            ReportQueryObject? query,
            Expression<Func<FinancialTransaction, TGroupKey>> groupBySelector,
            Func<TGroupKey, ReportKey> keySelector)
        {
            var transactions = GetFilteredTransactions(query);

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