using api.Constants;
using api.Dtos.Account;
using api.Models;
using api.Responses;
using api.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var user = new AppUser()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };
            var createdUser = await _userManager.CreateAsync(user, registerDto.Password);
            if (!createdUser.Succeeded)
            {
                var textError = string.Join("; ", createdUser.Errors.Select(e => e.Description));
                _logger.LogWarning("User registration failed: {Errors}", textError);
                return ApiResponse.BadRequest<NewUserDto>(textError);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);

            if (!roleResult.Succeeded)
            {
                var textError = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code} = {e.Description}"));
                _logger.LogError("Failed to assign role to user: {Errors}", textError);
                throw new Exception(textError);
            }

            _logger.LogDebug("Role '{Role}' assigned successfully", Roles.User);

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var plainRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenEntity = _tokenService.GenerateRefreshTokenEntity(plainRefreshToken, user, GetClientIp());
            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            _logger.LogDebug("Refresh token saved for user");

            var userDto = new NewUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
            };

            SetRefreshTokenCookie(plainRefreshToken, refreshTokenEntity.ExpiresAt);

            _logger.LogInformation("User registered successfully");

            return ApiResponse.Success(userDto);
        }

        [HttpPost("login")]
        public async Task<ApiResponse<NewUserDto>> Login([FromBody] LoginDto loginDto)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found");

                return ApiResponse.Unauthorized<NewUserDto>("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Invalid password");

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

            _logger.LogInformation("User logged in successfully");

            return ApiResponse.Success(newUserDto);
        }

        [HttpPost("refresh")]
        public async Task<ApiResponse<RefreshResponseDto>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No refresh token found in cookies");
                return ApiResponse.Unauthorized<RefreshResponseDto>("No refresh token");
            }

            var result = await _tokenService.TryRefreshTokensAsync(refreshToken, GetClientIp());

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Refresh token validation failed. Reason = {Error}", result.Error);
                return ApiResponse.Unauthorized<RefreshResponseDto>(result.Error!);
            }

            SetRefreshTokenCookie(result.NewRefreshToken, result.ExpiresAt!.Value);

            _logger.LogInformation("Refresh token successfully updated");
            return ApiResponse.Success(new RefreshResponseDto
            {
                Token = result.NewAccessToken!,
            });
        }

        [HttpPost("logout")]
        public async Task<ApiResponse<object>> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken, GetClientIp());
                Response.Cookies.Delete("refreshToken");
            }
            else
            {
                _logger.LogWarning("No refresh token found");
            }

            _logger.LogInformation("Logout completed");

            return ApiResponse.Success<object>(null);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ApiResponse<UserProfileDto>> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("No matching user found");

                return ApiResponse.Unauthorized<UserProfileDto>("Unauthorized");
            }

            _logger.LogInformation("Profile returned successfully");

            var profileDto = new UserProfileDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                CreatedAt = user.CreatedAt,
            };
            return ApiResponse.Success(profileDto);
        }

        private void SetRefreshTokenCookie(string token, DateTimeOffset expires)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = expires.UtcDateTime,
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
