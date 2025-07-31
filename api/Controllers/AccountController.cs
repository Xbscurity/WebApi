using api.Constants;
using api.Dtos.Account;
using api.Helpers;
using api.Models;
using api.Services.Token;
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

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService,
            SignInManager<AppUser> signInManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _env = env;
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
                return ApiResponse.BadRequest<NewUserDto>(createdUser.Errors.Aggregate(string.Empty, (acc, e) => acc + $"{e.Description}; "));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);

            if (!roleResult.Succeeded)
            {
                throw new Exception(roleResult.Errors.Aggregate(string.Empty, (acc, e) => acc + $"{e.Code} = {e.Description}; "));
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
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);
            if (user == null)
            {
                return ApiResponse.Unauthorized<NewUserDto>("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
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
                return ApiResponse.Unauthorized<RefreshResponseDto>("No refresh token");
            }

            var result = await _tokenService.TryRefreshTokensAsync(refreshToken, GetClientIp());

            if (!result.IsSuccess)
            {
                return ApiResponse.Unauthorized<RefreshResponseDto>(result.Error!);
            }

            SetRefreshTokenCookie(result.NewRefreshToken, result.ExpiresAt!.Value);

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

            return ApiResponse.Success<object>(null);
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
