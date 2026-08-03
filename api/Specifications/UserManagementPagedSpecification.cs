using api.Dtos.User;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Sorts and paginates <see cref="AppUser"/> entities for admin user management,
    /// projecting results to <see cref="UserManagementUserOutputDto"/>.
    /// </summary>
    public class UserManagementPagedSpecification : Specification<AppUser, UserManagementUserOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for sorting and pagination.</param>
        public UserManagementPagedSpecification(UserManagementQuery query)
        {
            var sortBy = query.SortBy?.Trim().ToLowerInvariant();
            switch (sortBy)
            {
                case "username":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(u => u.UserName)
                            .ThenByDescending(u => u.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(u => u.UserName)
                            .ThenByDescending(u => u.CreatedAt);
                    }

                    break;

                case "email":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(u => u.Email)
                            .ThenByDescending(u => u.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(u => u.Email)
                            .ThenByDescending(u => u.CreatedAt);
                    }

                    break;
                case "isbanned":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(u => u.IsBanned)
                            .ThenByDescending(u => u.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(u => u.IsBanned)
                            .ThenByDescending(u => u.CreatedAt);
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
            .Take(query.Size)
            .Select(u => new UserManagementUserOutputDto
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