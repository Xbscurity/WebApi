using api.Constants;
using api.Dtos.Account;
using api.Extensions;
using api.Models;
using api.Responses;
using api.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService,
            SignInManager<AppUser> signInManager, IWebHostEnvironment env,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _env = env;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ApiResponse<NewUserDto>> Register([FromBody] RegisterDto registerDto)
        {
            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };

            var createdUser = await _userManager.CreateAsync(user, registerDto.Password);

            if (!createdUser.Succeeded)
            {
                var errors = createdUser.Errors.Select(e => new { e.Code, e.Description });

                _logger.LogWarning(LoggingEvents.Users.Common.RegisterFailed, "Failed to create user: {@Errors}", errors);
                return ApiResponse.BadRequest<NewUserDto>("Registration failed", errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => new { e.Code, e.Description });

                _logger.LogError(LoggingEvents.Users.Common.RoleAssignFailed, "Failed to assign role to user: {@Errors}", errors);
                return ApiResponse.BadRequest<NewUserDto>("Failed to assign role", errors);
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var plainRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(plainRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            var userDto = new NewUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
            };

            SetRefreshTokenCookie(plainRefreshToken, refreshTokenEntity.ExpiresAt);

            return ApiResponse.Success(userDto);
        }

        [HttpPost("login")]
        public async Task<ApiResponse<NewUserDto>> Login([FromBody] LoginDto loginDto)
        {

            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.LoginUserNotFound, "User not found");

                return ApiResponse.Unauthorized<NewUserDto>("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.LoginInvalidPassword, "Invalid password");

                return ApiResponse.Unauthorized<NewUserDto>("Invalid username or password");
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var plainRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(plainRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);
            var newUserDto = new NewUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
            };

            SetRefreshTokenCookie(plainRefreshToken, refreshTokenEntity.ExpiresAt);

            return ApiResponse.Success(newUserDto);
        }

        [HttpPost("refresh")]
        public async Task<ApiResponse<RefreshResponseDto>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning(LoggingEvents.Users.Common.RefreshTokenMissing, "No refresh token found in cookies");
                return ApiResponse.Unauthorized<RefreshResponseDto>("No refresh token found");
            }

            var result = await _tokenService.TryRefreshTokensAsync(refreshToken, GetClientIp());

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    LoggingEvents.Users.Common.RefreshTokenInvalid,
                    "Refresh token validation failed. Reason = {Error}",
                    result.Error);
                return ApiResponse.Unauthorized<RefreshResponseDto>(result.Error!);
            }

            SetRefreshTokenCookie(result.NewRefreshToken, result.ExpiresAt!.Value);

            return ApiResponse.Success(new RefreshResponseDto
            {
                Token = result.NewAccessToken!,
            });
        }

        [HttpPost("logout")]
        public async Task<ApiResponse<string>> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken, GetClientIp(), "Logout");
                Response.Cookies.Delete("refreshToken");
            }

            return ApiResponse.Success("Logout completed");
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ApiResponse<UserProfileDto>> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            var profileDto = new UserProfileDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                CreatedAt = user.CreatedAt,
            };

            return ApiResponse.Success(profileDto);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ApiResponse<string>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordCheck)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.ChangePasswordCurrentFailed, "Invalid current password");
                return ApiResponse.BadRequest<string>("Unable to change password");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new { e.Code, e.Description }).ToList();

                _logger.LogWarning(LoggingEvents.Users.Common.ChangePasswordNewFailed, "Change password failed: {@Errors}", errors);
                return ApiResponse.BadRequest<string>("Failed to change password", errors);
            }

            var userId = User.GetUserId();
            await _tokenService.RevokeAllRefreshTokensAsync(userId, GetClientIp(), "Password changed");

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(newRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            SetRefreshTokenCookie(newRefreshToken);

            _logger.LogInformation(LoggingEvents.Users.Common.ChangePasswordSuccess, "Password changed successfully");
            return ApiResponse.Success("Password changed successfully");
        }

        private void SetRefreshTokenCookie(string token, DateTimeOffset expires = default)
        {
            var expiry = (expires == default) ? DateTimeOffset.UtcNow.AddDays(7) : expires;

            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = expiry,
            });
        }

        private string? GetClientIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            if (ip == null)
            {
                return null;
            }

            return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
        }

    }
}
