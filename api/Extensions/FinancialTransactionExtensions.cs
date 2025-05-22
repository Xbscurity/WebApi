using api.Dtos.FinancialTransactions;
using api.Models;
using api.Providers.Interfaces;

namespace api.Extensions
{
    public static class FinancialTransactionExtensions
    {
        public static FinancialTransactionOutputDto ToOutputDto(this FinancialTransaction financialTransaction)
        {
            if (financialTransaction is null)
            {
                return null;
            }

            return new FinancialTransactionOutputDto()
            {
                Id = financialTransaction.Id,
                CategoryName = financialTransaction.Category?.Name,
                Amount = financialTransaction.Amount,
                Comment = financialTransaction.Comment,
                CreatedAt = financialTransaction.CreatedAt,
            };
        }

        public static FinancialTransaction ToModel(this FinancialTransactionInputDto transactionInputDto, ITimeProvider timeProvider)
        {
            if (transactionInputDto is null)
            {
                return null;
            }

            return new FinancialTransaction(timeProvider)
            {
                Comment = transactionInputDto.Comment.Trim(),
                Amount = transactionInputDto.Amount,
                CategoryId = transactionInputDto.CategoryId,
            };
        }
    }
}
