using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using api.Data;
using api.Dtos;
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
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool groupByCategory = false,
            [FromQuery] bool groupByMonth = false)
        {
            var transactions = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .AsQueryable();
            if (startDate != null)
            {
                transactions = transactions.Where(t => t.Date >= startDate);
            }
            if (endDate != null)
            {
                transactions = transactions.Where(t => t.Date <= endDate);
            }
            if (groupByCategory && groupByMonth)
            {
                var transactionsReport = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(yearAndMonthGroups => new
                {
                    yearAndMonthGroups.Key.Year,
                    yearAndMonthGroups.Key.Month,
                    Categories = yearAndMonthGroups
                    .GroupBy(YearAndMonthGroup => YearAndMonthGroup.Category.Name ?? "No category")
                    .Select(categoryGroup => new
                    {
                      Category = categoryGroup.Key,
                      Transactions = categoryGroup.Select(transaction => new
                      {
                         Date = transaction.Date.ToString("yyyy-MM-dd"),
                        transaction.Amount,
                        transaction.Comment
                      })
                    })

                }).ToList();
                return Ok(transactionsReport);
            }
            else if(groupByCategory)
            {
                var  transactionsReport = transactions
                .GroupBy(transactions => transactions.Category.Name ?? "No category")
                .Select(transactionsGroup => new
                {
                    Category = transactionsGroup.Key ?? "No category",
                    Transactions = transactionsGroup.Select(transaction => new
                    {
                        Date = transaction.Date.ToString("yyyy-MM-dd"),
                        transaction.Comment,
                        transaction.Amount

                    })
                }).ToList();
                return Ok(transactionsReport);
            } else if(groupByMonth)
            {
                var  transactionsReport = transactions
                .GroupBy(transactions => new {transactions.Date.Year, transactions.Date.Month})
                .Select(transactionsGroup => new
                {
                    transactionsGroup.Key.Year,
                    transactionsGroup.Key.Month,
                    Transactions = transactionsGroup.Select(transaction => new
                    {
                        Category = transaction.Category.Name ?? "No category",
                        transaction.Amount,
                        transaction.Comment

                    })
                }).ToList();
            return Ok(transactionsReport);
            }
            var transactionReport = transactions.Select(transaction => new //default grouping if no grouping selected
            {
                Date = transaction.Date.ToString("yyyy-MM-dd"),
                Category = transaction.Category.Name ?? "No category",
                transaction.Amount,
                transaction.Comment

            });
            return Ok(transactionReport);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category).ToListAsync();
            return Ok(transactions);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound($"Transaction with ID {id} not found");
            }
            return Ok(transaction);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialTransactionDto transactionDto)
        {
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return BadRequest($"Category with ID {transactionDto.CategoryId} not found");
            }
            FinancialTransaction transaction = new FinancialTransaction
            {
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Comment = transactionDto.Comment
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] FinancialTransactionDto transactionDto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound($"Transaction with ID {id} not found");
            }
            var existingCategory = await _context.Categories.FindAsync(transactionDto.CategoryId);
            if (existingCategory == null)
            {
                return BadRequest($"Category with ID {transactionDto.CategoryId} not found");
            }
            transaction.Amount = transactionDto.Amount;
            transaction.CategoryId = transactionDto.CategoryId;
            transaction.Comment = transactionDto.Comment;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound($"Transaction with ID {id} not found");
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}