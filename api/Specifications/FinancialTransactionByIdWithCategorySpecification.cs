using api.Dtos.FinancialTransaction;
using api.Models;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Retrieves a specific <see cref="FinancialTransaction"/> entity by its identifier
    /// for a specific user and projects the result to <see cref="FinancialTransactionOutputDto"/>.
    /// </summary>
    public class FinancialTransactionByIdWithCategorySpecification
    : Specification<FinancialTransaction, FinancialTransactionOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionByIdWithCategorySpecification"/> class.
        /// </summary>
        /// <param name="id">The identifier of the financial transaction to retrieve.</param>
        /// <param name="userId">The identifier of the user who owns the financial transaction.</param>
        public FinancialTransactionByIdWithCategorySpecification(Guid id, string userId)
        {
            Query
                .Where(t => t.AppUserId == userId)
                .Where(t => t.Id == id)
                .Select(t => new FinancialTransactionOutputDto
                {
                    Id = t.Id,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    Amount = t.Amount,
                    Type = t.Type,
                    Comment = t.Comment,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                });
        }
    }
}