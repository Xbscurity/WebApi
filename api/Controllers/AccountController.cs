using api.Dtos.Account;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using api.Constants;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using api.Dtos.UserManagement;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ApiResponse<NewUserDto>> Register([FromBody] RegisterDto registerDto)
        {
            var appUser = new AppUser()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };
            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (!createdUser.Succeeded)
            {
                return ApiResponse.BadRequest<NewUserDto>(createdUser.Errors.Aggregate(string.Empty, (acc, e) => acc + $"{e.Description}; "));
            }

            var roleResult = await _userManager.AddToRoleAsync(appUser, Roles.User);

            if (!roleResult.Succeeded)
                {
                    throw new Exception(roleResult.Errors.Aggregate(string.Empty, (acc, e) => acc + $"{e.Code} = {e.Description}; "));
                }

            var userDto = new NewUserDto
                    {
                        UserName = registerDto.UserName,
                        Email = registerDto.Email,
                        Token = await _tokenService.CreateToken(appUser),
                    };
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
            var newUserDto = new NewUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
            };
            return ApiResponse.Success(newUserDto);
        }
    }
}
