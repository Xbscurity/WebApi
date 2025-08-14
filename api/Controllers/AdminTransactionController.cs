using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.QueryObjects;
using api.Responses;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/admin/transactions")]
    public class AdminTransactionController : BaseTransactionController
    {
        public AdminTransactionController(ITransactionService transactionService)
            : base(transactionService)
        {
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

            var transactions = await _transactionService.GetAllForAdminAsync(queryObject);
            return ApiResponse.Success(transactions.Data, transactions.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Create([FromBody] AdminFinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionService.CreateForAdminAsync(User, transactionDto);
            return ApiResponse.Success(result);
        }
    }
}
