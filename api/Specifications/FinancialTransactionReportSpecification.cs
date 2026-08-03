using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters <see cref="FinancialTransaction"/> entities for a specific user during report generation,
    /// including related <see cref="Category"/> data.
    /// </summary>
    public class FinancialTransactionReportSpecification : Specification<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReportSpecification"/> class.
        /// </summary>
        /// <param name="query">
        /// Report query parameters for date range and activity filtering.
        /// </param>
        /// <param name="userId">
        /// The identifier of the user whose financial transactions are included in the report.
        /// </param>
        public FinancialTransactionReportSpecification(ReportQuery query, string userId)
        {
            Query
                .Where(c => c.AppUserId == userId)
                .Include(ft => ft.Category);

            if (!query.IncludeInactive)
            {
                Query.Where(c => c.Category.IsActive);
            }

            if (query.StartDate != null)
            {
                Query.Where(t => t.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate != null)
            {
                Query.Where(t => t.CreatedAt <= query.EndDate.Value);
            }
        }
    }
}