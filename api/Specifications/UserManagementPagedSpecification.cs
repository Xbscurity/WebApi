using api.Dtos.User;
using api.Models;
using api.QueryObjects;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Specification for retrieving paginated and sorted users for management purposes.
    /// </summary>
    public class UserManagementPagedSpecification : Specification<AppUser, UserManagementUserOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for sorting and pagination.</param>
        /// <remarks>
        /// Applies:
        /// <list type="bullet">
        /// <item><description>Sorting by ban status or creation date</description></item>
        /// <item><description>Pagination using page number and size</description></item>
        /// <item><description>Projection to <see cref="UserManagementUserOutputDto"/></description></item>
        /// </list>
        /// </remarks>
        public UserManagementPagedSpecification(UserManagementQuery query)
        {
            var sortBy = query.SortBy.Trim().ToLowerInvariant();
            switch (sortBy)
            {
                case "isbanned":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(u => u.IsBanned);
                    }
                    else
                    {
                        Query.OrderBy(u => u.IsBanned);
                    }

                    break;
                case "createdat":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(u => u.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(u => u.CreatedAt);
                    }

                    break;

                default:

                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(u => u.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(u => u.CreatedAt);
                    }

                    break;
            }

            Query
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);

            Query.Select(u => new UserManagementUserOutputDto
            {
                Id = u.Id,
                Email = u.Email!,
                UserName = u.UserName!,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt,
            });
        }
    }
}