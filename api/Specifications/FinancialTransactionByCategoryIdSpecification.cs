using api.Models;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Specification for filtering financial transactions by category identifier.
    /// </summary>
    public class FinancialTransactionByCategoryIdSpecification : Specification<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionByCategoryIdSpecification"/> class.
        /// </summary>
        /// <param name="id">The category identifier to filter transactions by.</param>
        public FinancialTransactionByCategoryIdSpecification(Guid id)
        {
            Query.Where(ft => ft.CategoryId == id);
        }
    }
}
