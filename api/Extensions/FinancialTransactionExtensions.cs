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
            return new FinancialTransactionOutputDto(
                financialTransaction.Id,
                financialTransaction.Category?.Name,
                financialTransaction.Amount,
                financialTransaction.Comment,
                financialTransaction.CreatedAt
            );
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
                CategoryId = transactionInputDto.CategoryId
            };
        }
    }
}
