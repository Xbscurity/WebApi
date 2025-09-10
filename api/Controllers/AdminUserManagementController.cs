using api.Constants;
using api.Dtos.UserManagement;
using api.Extensions;
using api.Models;
using api.QueryObjects;
using api.Responses;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [Route("api/account")]
    [ApiController]
    public class AdminUserManagementController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminUserManagementController> _logger;
        private readonly UserSortValidator _sortValidator;

        public AdminUserManagementController(
            UserManager<AppUser> userManager,
            ILogger<AdminUserManagementController> logger,
            UserSortValidator sortValidator)
        {
            _userManager = userManager;
            _logger = logger;
            _sortValidator = sortValidator;
        }

        [HttpPost("ban-status/{userId}")]
        public async Task<ApiResponse> SetBanStatus(string userId, [FromBody] BanStatusDto request)
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

        [HttpGet("users")]
        public async Task<ApiResponse<List<UserDto>>> GetAllUsers([FromQuery] PaginationQueryObject queryObject)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<UserDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var query = await _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    IsBanned = u.IsBanned,
                })
                .ToPagedQueryAsync(queryObject);

            var result = new PagedData<UserDto>
            {
                Data = await query.Query.ToListAsync(),
                Pagination = query.Pagination,
            };
            _logger.LogInformation(LoggingEvents.Users.Admin.GetAll, "Returning {Count} users", result.Data.Count);
            return ApiResponse.Success(result.Data, result.Pagination);
        }
    }
}
