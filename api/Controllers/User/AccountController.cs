using api.Constants;
using api.Dtos.Account;
using api.Extensions;
using api.Models;
using api.Responses;
using api.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.User
{
    /// <summary>
    /// Provides endpoints for account management such as registration, login,
    /// token refresh, logout, profile retrieval, and password changes.
    /// </summary>
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">Provides APIs for managing users in the identity system.</param>
        /// <param name="tokenService">Service responsible for generating and validating access/refresh tokens.</param>
        /// <param name="signInManager">Provides sign-in operations for users.</param>
        /// <param name="env">Provides information about the web hosting environment (e.g., Development, Production).</param>
        /// <param name="logger">Logger instance for recording diagnostic and error information.</param>
        public AccountController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            IWebHostEnvironment env,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user and issues authentication tokens.
        /// </summary>
        /// <param name="registerDto">The registration data (username, email, password).</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the newly created user information and tokens,
        /// or an error response if registration fails.
        /// </returns>
        [HttpPost("register")]
        public async Task<ApiResponse<AccountUserOutputDto>> Register([FromBody] RegisterInputDto registerDto)
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

                _logger.LogWarning(LoggingEvents.Users.Common.Register, "Failed to create user: {@Errors}", errors);
                return ApiResponse.BadRequest<AccountUserOutputDto>("Registration failed", errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => new { e.Code, e.Description });

                _logger.LogError(LoggingEvents.Users.Common.Register, "Failed to assign role to user: {@Errors}", errors);
                return ApiResponse.BadRequest<AccountUserOutputDto>("Failed to assign role", errors);
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var plainRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(plainRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            var userDto = new AccountUserOutputDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
            };

            SetRefreshTokenCookie(plainRefreshToken, refreshTokenEntity.ExpiresAt);

            return ApiResponse.Success(userDto);
        }

        /// <summary>
        /// Authenticates an existing user and issues new tokens.
        /// </summary>
        /// <param name="loginDto">The login data (username, password).</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing user information and tokens if authentication succeeds,
        /// or an unauthorized response if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        public async Task<ApiResponse<AccountUserOutputDto>> Login([FromBody] LoginInputDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.Login, "User not found");

                return ApiResponse.Unauthorized<AccountUserOutputDto>("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.Login, "Invalid password");

                return ApiResponse.Unauthorized<AccountUserOutputDto>("Invalid username or password");
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var plainRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(plainRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);
            var newUserDto = new AccountUserOutputDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                Token = accessToken,
            };

            SetRefreshTokenCookie(plainRefreshToken, refreshTokenEntity.ExpiresAt);

            return ApiResponse.Success(newUserDto);
        }

        /// <summary>
        /// Refreshes the authentication tokens using a valid refresh token cookie.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a new access token (and updated refresh token in cookies),
        /// or an unauthorized response if the refresh token is missing or invalid.
        /// </returns>
        [HttpPost("refresh")]
        public async Task<ApiResponse<RefreshOutputDto>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning(LoggingEvents.Users.Common.RefreshToken, "No refresh token found in cookies");
                return ApiResponse.Unauthorized<RefreshOutputDto>("No refresh token found");
            }

            var result = await _tokenService.TryRefreshTokensAsync(refreshToken, GetClientIp());

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    LoggingEvents.Users.Common.RefreshToken,
                    "Refresh token validation failed. Reason = {Error}",
                    result.Error);
                return ApiResponse.Unauthorized<RefreshOutputDto>(result.Error!);
            }

            SetRefreshTokenCookie(result.NewRefreshToken!, result.ExpiresAt!.Value);

            return ApiResponse.Success(new RefreshOutputDto
            {
                Token = result.NewAccessToken!,
            });
        }

        /// <summary>
        /// Logs out the user by revoking the current refresh token and clearing the cookie.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> with a confirmation message when logout is completed.
        /// </returns>
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

        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the user's profile details.
        /// </returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ApiResponse<UserProfileOutputDto>> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.ChangePassword, "User not found");
                return ApiResponse.Unauthorized<UserProfileOutputDto>();
            }

            var profileDto = new UserProfileOutputDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                CreatedAt = user.CreatedAt,
            };

            return ApiResponse.Success(profileDto);
        }

        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="dto">The password change request containing the current and new passwords.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> with a success message if the password was changed,
        /// or an error response if validation fails.
        /// </returns>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ApiResponse<string>> ChangePassword([FromBody] ChangePasswordInputDto dto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.ChangePassword, "User not found");
                return ApiResponse.Unauthorized<string>();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordCheck)
            {
                _logger.LogWarning(LoggingEvents.Users.Common.ChangePassword, "Invalid current password");
                return ApiResponse.BadRequest<string>("Unable to change password");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new { e.Code, e.Description }).ToList();

                _logger.LogWarning(LoggingEvents.Users.Common.ChangePassword, "Change password failed: {@Errors}", errors);
                return ApiResponse.BadRequest<string>("Failed to change password", errors);
            }

            var userId = User.GetUserId();
            await _tokenService.RevokeAllRefreshTokensAsync(userId!, GetClientIp(), "Password changed");

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(newRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            SetRefreshTokenCookie(newRefreshToken);

            return ApiResponse.Success("Password changed successfully");
        }

        /// <summary>
        /// Sets the refresh token cookie with proper security options.
        /// </summary>
        private void SetRefreshTokenCookie(string token, DateTimeOffset expires = default)
        {
            var expiry = expires == default ? DateTimeOffset.UtcNow.AddDays(7) : expires;

            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = expiry,
            });
        }

        /// <summary>
        /// Gets the client's IP address, mapped to IPv4 if necessary.
        /// </summary>
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
