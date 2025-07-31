using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseTransactionController : ControllerBase
    {
        protected readonly ITransactionService _transactionService;

        public BaseTransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{id:int}")]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> GetById([FromRoute] int id)
        {
            var transaction = await _transactionService.GetByIdAsync(User, id);
            if (transaction == null)
            {
                return ApiResponse.NotFound<BaseFinancialTransactionOutputDto>("Transaction not found");
            }

            return ApiResponse.Success(transaction);
        }

        [HttpPut("{id:int}")]
        public async Task<ApiResponse<BaseFinancialTransactionOutputDto>> Update([FromRoute] int id, [FromBody] BaseFinancialTransactionInputDto transactionDto)
        {
            var result = await _transactionService.UpdateAsync(User, id, transactionDto);
            if (result is null)
            {
                return ApiResponse.NotFound<BaseFinancialTransactionOutputDto>("Transaction not found");
            }

            return ApiResponse.Success(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            bool result = await _transactionService.DeleteAsync(User, id);
            if (result is false)
            {
                return ApiResponse.NotFound<bool>("Transaction not found");
            }

            return ApiResponse.Success(result);
        }
    }
}
