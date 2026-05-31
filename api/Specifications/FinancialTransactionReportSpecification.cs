using api.Models;
using api.Providers.CurrentUser;
using api.QueryObjects;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Specification for generating financial transaction reports with filtering and related data inclusion.
    /// </summary>
    public class FinancialTransactionReportSpecification : Specification<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReportSpecification"/> class.
        /// </summary>
        /// <param name="query">Report query parameters for filtering.</param>
        /// <param name="currentUser">The current user context.</param>
        /// <remarks>
        /// Applies:
        /// <list type="bullet">
        /// <item><description>Date range filtering (start and end dates)</description></item>
        /// <item><description>User-based filtering (admin can query by user, others limited to their own data)</description></item>
        /// <item><description>Includes related category data</description></item>
        /// <item><description>Optional filtering of inactive categories</description></item>
        /// </list>
        /// </remarks>
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

            if (!currentUser.IsAdmin)
            {
                Query.Where(c => c.AppUserId == currentUser.UserId);
            }
            else if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                Query.Where(c => c.AppUserId == query.UserId);
            }

            Query.Include(ft => ft.Category);

            if (!query.IncludeInactive)
            {
                Query.Where(c => c.Category.IsActive);
            }
        }
    }
}
