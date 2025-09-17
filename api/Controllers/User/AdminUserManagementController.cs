using api.Constants;
using api.Dtos.User;
using api.Extensions;
using api.Models;
using api.QueryObjects;
using api.Responses;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers.User
{
    /// <summary>
    /// Provides API endpoints for administrators to manage user accounts,
    /// including banning/unbanning users and retrieving the list of users.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [Route("api/account")]
    [ApiController]
    public class AdminUserManagementController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminUserManagementController> _logger;
        private readonly UserSortValidator _sortValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUserManagementController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager responsible for user operations.</param>
        /// <param name="logger">The logger for recording user management events.</param>
        /// <param name="sortValidator">Validates sorting fields for user queries.</param>
        public AdminUserManagementController(
            UserManager<AppUser> userManager,
            ILogger<AdminUserManagementController> logger,
            UserSortValidator sortValidator)
        {
            _userManager = userManager;
            _logger = logger;
            _sortValidator = sortValidator;
        }

        /// <summary>
        /// Updates the ban status of a specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user to update.</param>
        /// <param name="request">The ban status request data.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating the result of the operation:
        /// <list type="bullet">
        /// <item><description><c>Success</c> – if the ban status was updated successfully.</description></item>
        /// <item><description><c>BadRequest</c> – if the user is already in the requested state or if the target is an administrator.</description></item>
        /// <item><description><c>NotFound</c> – if the user does not exist.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Administrators cannot be banned using this endpoint.
        /// </remarks>
        [HttpPost("ban-status/{userId}")]
        public async Task<ApiResponse> SetBanStatus(string userId, [FromBody] BanStatusInputDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.Users.Admin.NotFound, "User not found");
                return ApiResponse.NotFound<string>("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Admin))
            {
                _logger.LogWarning(LoggingEvents.Users.Admin.AdminBanAttempt, "Attempt of banning administrator");
                return ApiResponse.BadRequest<string>("You cannot modify the ban status of an administrator");
            }

            if (user.IsBanned == request.IsBanned)
            {
                _logger.LogWarning(
                    LoggingEvents.Users.Admin.UserAlreadyBanned,
                    "User is already {BanStatus}",
                    request.IsBanned ? "banned" : "unbanned");
                return ApiResponse.BadRequest<string>($"User is already {(request.IsBanned ? "banned" : "unbanned")}");
            }

            user.IsBanned = request.IsBanned;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation(
                LoggingEvents.Users.Admin.UserBanned,
                "User has been {BanStatus}",
                request.IsBanned ? "banned" : "unbanned");
            return ApiResponse.Success($"User has been {(request.IsBanned ? "banned" : "unbanned")}");
        }

        /// <summary>
        /// Retrieves a paginated list of all users in the system.
        /// </summary>
        /// <param name="queryObject">
        /// The pagination and sorting parameters for retrieving users.
        /// Sorting is validated using <see cref="UserSortValidator"/>.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of <see cref="AdminUserManagementUserOutputDto"/> objects.  
        /// Returns <see cref="ApiResponse.BadRequest"/> if the provided <c>SortBy</c> parameter is invalid.
        /// </returns>
        [HttpGet("users")]
        public async Task<ApiResponse<List<AdminUserManagementUserOutputDto>>> GetAllUsers([FromQuery] PaginationQueryObject queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<AdminUserManagementUserOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var query = await _userManager.Users
                .Select(u => new AdminUserManagementUserOutputDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    IsBanned = u.IsBanned,
                })
                .ToPagedQueryAsync(queryObject);

            var result = new PagedData<AdminUserManagementUserOutputDto>
            {
                Data = await query.Query.ToListAsync(),
                Pagination = query.Pagination,
            };
            _logger.LogInformation(LoggingEvents.Users.Admin.GetAll, "Returning {Count} users", result.Data.Count);
            return ApiResponse.Success(result.Data, result.Pagination);
        }
    }
}
