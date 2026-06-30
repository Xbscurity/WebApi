using api.Constants;
using api.Dtos.User;
using api.Queries;
using api.Services.Shared;
using api.Services.User;
using api.Specifications;
using ErrorOr;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Frozen;

namespace api.Services.UserManagement
{
    /// <summary>
    /// Default implementation of <see cref="IUserManagementService"/>.
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        /// <summary>
        /// List of allowed fields for sorting user queries.
        /// </summary>
        private static readonly FrozenSet<string> ValidFields = new[]
        {
            "username",
            "email",
            "isBanned",
            "createdat",
        }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly IUserService _userService;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementService"/> class.
        /// </summary>
        /// <param name="userService">User service abstraction.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="cache">Memory cache for user-related data.</param>
        public UserManagementService(IUserService userService, ILogger<UserManagementService> logger, IMemoryCache cache)
        {
            _userService = userService;
            _logger = logger;
            _cache = cache;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<PagedItems<UserManagementUserOutputDto>>> GetAllUsersAsync(UserManagementQuery query)
        {
            if (!ValidFields.Contains(query.SortBy))
            {
                var allowed = string.Join(", ", ValidFields);

                _logger.LogWarning(
                    LoggingEvents.User.SortInvalid,
                    "SortBy '{Field}' is invalid. Allowed fields: {AllowedFields}",
                    query.SortBy,
                    allowed);

                return Errors.User.InvalidSortBy(query.SortBy, ValidFields);
            }

            var spec = new UserManagementPagedSpecification(query);
            var users = await _userService.GetAllAsync(spec);
            var count = await _userService.CountAsync();

            var pagination = new Pagination(query.Page, query.Size, count);

            var pagedData = new PagedItems<UserManagementUserOutputDto>
            {
                Items = users,
                Pagination = pagination,
            };

            _logger.LogDebug("Returning {Count} users", pagedData.Items.Count);
            return pagedData;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<UserManagementUserOutputDto>> GetByIdAsync(string id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user == null)
            {
                return Errors.User.NotFound(id);
            }

            return new UserManagementUserOutputDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsBanned = user.IsBanned,
            };
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<BanStatusOutputDto>> SetBanAsync(string userId, BanStatusInputDto inputDto)
        {
            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.User.NotFound, "User not found");
                return Errors.User.NotFound(userId);
            }

            var roles = await _userService.GetRolesAsync(user);
            if (roles.Contains(Roles.Admin))
            {
                _logger.LogWarning(LoggingEvents.User.AdminBanAttempt, "Attempt of banning administrator");
                return Errors.User.AdminBanAttempt(userId);
            }

            user.IsBanned = inputDto.IsBanned;

            var updateResult = await _userService.UpdateAsync(user);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }

            _cache.Remove(UserCacheKeys.BanStatus(userId));

            return new BanStatusOutputDto
            {
                BanStatus = user.IsBanned,
            };
        }
    }
}