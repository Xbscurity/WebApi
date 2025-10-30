using api.Dtos.FinancialTransaction;
using api.Models;
using api.Providers.Interfaces;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for mapping between <see cref="FinancialTransaction"/> entities
    /// and their corresponding Data Transfer Objects (DTOs).
    /// </summary>
    public static class FinancialTransactionExtensions
    {
        /// <summary>
        /// Converts a <see cref="FinancialTransaction"/> entity to a <see cref="BaseFinancialTransactionOutputDto"/>.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction entity to convert.</param>
        /// <returns>
        /// A <see cref="BaseFinancialTransactionOutputDto"/> representation of the entity,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static BaseFinancialTransactionOutputDto ToOutputDto(this FinancialTransaction financialTransaction)
        {
            if (financialTransaction is null)
            {
                return null!;
            }

            return new BaseFinancialTransactionOutputDto()
            {
                Id = financialTransaction.Id,
                CategoryId = financialTransaction.Category.Id,
                CategoryName = financialTransaction.Category.Name,
                Amount = financialTransaction.Amount,
                Comment = financialTransaction.Comment,
                CreatedAt = financialTransaction.CreatedAt,
                AppUserId = financialTransaction.AppUserId,
            };
        }

        /// <summary>
        /// Converts a <see cref="BaseFinancialTransactionInputDto"/> to a <see cref="FinancialTransaction"/> entity.
        /// </summary>
        /// <param name="transactionInputDto">The input DTO containing transaction data.</param>
        /// <param name="appUserId">The identifier of the user who owns the transaction.</param>
        /// <param name="timeProvider">An implementation of <see cref="ITimeProvider"/> used to set the creation time.</param>
        /// <returns>
        /// A new <see cref="FinancialTransaction"/> entity populated with the DTO data,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static FinancialTransaction ToModel(this BaseFinancialTransactionInputDto transactionInputDto, string appUserId, ITimeProvider timeProvider)
        {
            if (transactionInputDto is null)
            {
                return null!;
            }

            return new FinancialTransaction(timeProvider)
            {
                Comment = transactionInputDto.Comment.Trim(),
                Amount = transactionInputDto.Amount,
                CategoryId = transactionInputDto.CategoryId,
                AppUserId = appUserId,
            };
        }

        /// <summary>
        /// Converts an <see cref="AdminFinancialTransactionInputDto"/> to a <see cref="FinancialTransaction"/> entity.
        /// </summary>
        /// <param name="transactionInputDto">The admin input DTO containing transaction data.</param>
        /// <param name="timeProvider">An implementation of <see cref="ITimeProvider"/> used to set the creation time.</param>
        /// <returns>
        /// A new <see cref="FinancialTransaction"/> entity populated with the DTO data,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static FinancialTransaction ToModel(this AdminFinancialTransactionInputDto transactionInputDto, ITimeProvider timeProvider)
        {
            if (transactionInputDto is null)
            {
                return null!;
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
