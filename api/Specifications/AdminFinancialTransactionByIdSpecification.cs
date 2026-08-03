using api.Dtos.FinancialTransaction;
using api.Models;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Retrieves a specific <see cref="FinancialTransaction"/> entity by its identifier,
    /// including category information and projecting the result to
    /// <see cref="AdminFinancialTransactionOutputDto"/> for administrative purposes.
    /// </summary>
    public class AdminFinancialTransactionByIdSpecification
        : Specification<FinancialTransaction, AdminFinancialTransactionOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFinancialTransactionByIdSpecification"/> class.
        /// </summary>
        /// <param name="id">The identifier of the financial transaction to retrieve.</param>
        public AdminFinancialTransactionByIdSpecification(Guid id)
        {
            Query
                .Where(t => t.Id == id)
                .Select(ft => new AdminFinancialTransactionOutputDto
                {
                    Id = ft.Id,
                    CategoryId = ft.CategoryId,
                    Amount = ft.Amount,
                    CategoryName = ft.Category.Name,
                    Type = ft.Type,
                    Comment = ft.Comment,
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt,
                    AppUserId = ft.AppUserId,
                });
        }
    }
}