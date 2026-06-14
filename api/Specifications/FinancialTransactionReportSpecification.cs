using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters <see cref="FinancialTransaction"/> entities for report generation,
    /// including related <see cref="Category"/> data.
    /// </summary>
    public class FinancialTransactionReportSpecification : Specification<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReportSpecification"/> class.
        /// </summary>
        /// <param name="query">Report query parameters for date range and activity filtering.</param>
        /// <param name="currentUser">The current user context.</param>
        public FinancialTransactionReportSpecification(ReportQuery query, ICurrentUser currentUser)
        {
            if (query.StartDate != null)
            {
                Query.Where(t => t.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate != null)
            {
                Query.Where(t => t.CreatedAt <= query.EndDate.Value);
            }

            Query.Where(c => c.AppUserId == currentUser.UserId);

            Query.Include(ft => ft.Category);

            if (!query.IncludeInactive)
            {
                Query.Where(c => c.Category.IsActive);
            }
        }
    }
}
