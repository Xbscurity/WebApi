using api.Dtos.FinancialTransaction;
using api.Models;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="FinancialTransaction"/> entities.
    /// </summary>
    public static class FinancialTransactionExtensions
    {
        /// <summary>
        /// Converts a <see cref="FinancialTransaction"/> entity
        /// into a <see cref="FinancialTransactionOutputDto"/>.
        /// </summary>
        /// <param name="financialTransaction">
        /// The financial transaction entity to convert.
        /// </param>
        /// <param name="categoryName">
        /// The name of the category assigned to the financial transaction.
        /// </param>
        /// <returns>
        /// A DTO representation of the financial transaction.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="financialTransaction"/> is <see langword="null"/>.
        /// </exception>
        public static FinancialTransactionOutputDto ToOutputDto(
            this FinancialTransaction financialTransaction, string categoryName)
        {
            ArgumentNullException.ThrowIfNull(financialTransaction);

            return new FinancialTransactionOutputDto()
            {
                Id = financialTransaction.Id,
                CategoryId = financialTransaction.CategoryId,
                CategoryName = categoryName,
                Amount = financialTransaction.Amount,
                Type = financialTransaction.Type,
                Comment = financialTransaction.Comment,
                CreatedAt = financialTransaction.CreatedAt,
                UpdatedAt = financialTransaction.UpdatedAt,
            };
        }

        /// <summary>
        /// Converts a <see cref="FinancialTransaction"/> entity
        /// into a <see cref="AdminFinancialTransactionOutputDto"/>.
        /// </summary>
        /// <param name="financialTransaction">
        /// The financial transaction entity to convert.
        /// </param>
        /// <param name="categoryName">
        /// The name of the category assigned to the financial transaction.
        /// </param>
        /// <returns>
        /// A DTO representation of the financial transaction.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="financialTransaction"/> is <see langword="null"/>.
        /// </exception>
        public static AdminFinancialTransactionOutputDto ToAdminOutputDto(
                this FinancialTransaction financialTransaction, string categoryName)
        {
            ArgumentNullException.ThrowIfNull(financialTransaction);

            return new AdminFinancialTransactionOutputDto()
            {
                Id = financialTransaction.Id,
                CategoryId = financialTransaction.CategoryId,
                CategoryName = categoryName,
                Amount = financialTransaction.Amount,
                Type = financialTransaction.Type,
                Comment = financialTransaction.Comment,
                CreatedAt = financialTransaction.CreatedAt,
                UpdatedAt = financialTransaction.UpdatedAt,
                AppUserId = financialTransaction.AppUserId,
            };
        }
    }
}
