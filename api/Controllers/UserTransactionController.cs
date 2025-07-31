using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.QueryObjects;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.UserNotBanned)]
    [ApiController]
    [Route("api/user/transactions")]
    public class UserTransactionController : BaseTransactionController
    {

        public UserTransactionController(ITransactionService transactionService)
            : base(transactionService)
        {
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
        ///
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
            var report = await _transactionService.GetReportAsync(User, queryObject);
            return ApiResponse.Success(report.Data, report.Pagination);
        }

        [HttpGet]
        public async Task<ApiResponse<List<BaseFinancialTransactionOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
              {
                  "id", "category", "amount", "date",
              };
            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<BaseFinancialTransactionOutputDto>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }
            var transactions = await _transactionService.GetAllForUserAsync(User, queryObject);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Create([FromBody] BaseFinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionService.CreateForUserAsync(User, transactionDto);
            return ApiResponse.Success(result);
        }
    }
}
