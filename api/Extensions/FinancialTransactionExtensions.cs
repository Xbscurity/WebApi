using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Models;
using api.Providers.Interfaces;

namespace api.Extensions
{
    public static class FinancialTransactionExtensions
    {
        public static BaseFinancialTransactionOutputDto ToOutputDto(this FinancialTransaction financialTransaction)
        {
            if (financialTransaction is null)
            {
                return null;
            }

            return new BaseFinancialTransactionOutputDto()
            {
                Id = financialTransaction.Id,
                CategoryName = financialTransaction.Category?.Name,
                Amount = financialTransaction.Amount,
                Comment = financialTransaction.Comment,
                CreatedAt = financialTransaction.CreatedAt,
                AppUserId = financialTransaction.AppUserId,
            };
        }

        public static FinancialTransaction ToModel(this BaseFinancialTransactionInputDto transactionInputDto, string appUserId, ITimeProvider timeProvider)
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
                AppUserId = appUserId,
            };
        }

        public static FinancialTransaction ToModel(this AdminFinancialTransactionInputDto transactionInputDto, ITimeProvider timeProvider)
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
                AppUserId = transactionInputDto.AppUserId,
            };
        }
    }
}
