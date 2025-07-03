using api.Constants;
using api.Dtos.UserManagement;
using api.Extensions;
using api.Helpers;
using api.Models;
using api.QueryObjects;
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

        public AdminUserManagementController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("ban-status/{userId}")]
        public async Task<ApiResponse> SetBanStatus(string userId, [FromBody] BanRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.NotFound<string>("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Admin))
            {
                return ApiResponse.BadRequest<string>("You cannot modify the ban status of an administrator");
            }

            if (user.IsBanned == request.IsBanned)
            {
                return ApiResponse.BadRequest<string>($"User is already {(request.IsBanned ? "banned" : "unbanned")}");
            }

            user.IsBanned = request.IsBanned;

            await _userManager.UpdateAsync(user);

            return ApiResponse.Success($"User has been {(request.IsBanned ? "banned" : "unbanned")}");
        }

        [HttpGet("users")]
        public async Task<ApiResponse<PagedData<UserDto>>> GetAllUsers([FromQuery] PaginationQueryObject queryObject)
        {
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

            return ApiResponse.Success(result);
        }
    }
}
