using api.Constants;
using api.Dtos.Account;
using api.Dtos.User;
using api.Services.Auth;
using api.Services.RefreshTokenCookie;
using api.Services.Token;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.User
{
    /// <summary>
    /// Provides authentication endpoints for user registration, login, token refresh, and logout.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;
        private readonly IRefreshTokenCookieService _refreshTokenCookieService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="tokenService">
        /// Service responsible for issuing and revoking tokens.
        /// </param>
        /// <param name="authService">
        /// Service responsible for authentication workflows such as login and registration.
        /// </param>
        /// <param name="refreshTokenCookieService">
        /// Service responsible for storing refresh tokens in HTTP-only cookies.
        /// </param>
        public AuthController(
            ITokenService tokenService,
            IAuthService authService,
            IRefreshTokenCookieService refreshTokenCookieService)
        {
            _tokenService = tokenService;
            _authService = authService;
            _refreshTokenCookieService = refreshTokenCookieService;
        }

        /// <summary>
        /// Registers a new user and returns authentication tokens.
        /// </summary>
        /// <param name="registerDto">The registration details for the new user.</param>
        /// <returns>
        /// Authentication data including access token and user information.
        /// </returns>
        /// <response code="200">
        /// The user was successfully registered.
        /// </response>
        [HttpPost("register")]
        public async Task<ActionResult<AuthOutputDto>> Register([FromBody] RegisterInputDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            var value = result.Value;
            _refreshTokenCookieService.Set(value.RefreshToken);
            var authDto = new AuthOutputDto
            {
                AccessToken = value.AccessToken,
                Email = value.Email,
                UserName = value.UserName,
            };
            return authDto;
        }

        /// <summary>
        /// Authenticates a user and returns authentication tokens.
        /// </summary>
        /// <param name="loginDto">The user login credentials.</param>
        /// <returns>
        /// Authentication data including access token and user information.
        /// </returns>
        /// <response code="200">
        /// The user was successfully authenticated.
        /// </response>
        [HttpPost("login")]
        public async Task<ActionResult<AuthOutputDto>> Login([FromBody] LoginInputDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            var value = result.Value;
            _refreshTokenCookieService.Set(value.RefreshToken);
            var authDto = new AuthOutputDto
            {
                AccessToken = value.AccessToken,
                Email = value.Email,
                UserName = value.UserName,
            };
            return authDto;
        }

        /// <summary>
        /// Refreshes the current authentication session using a refresh token cookie.
        /// </summary>
        /// <returns>
        /// A new access token and updated refresh token.
        /// </returns>
        /// <response code="200">
        /// The session was successfully refreshed.
        /// </response>
        /// <response code="401">
        /// The refresh token is missing, invalid, or expired.
        /// </response>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshTokenOutputDto>> Refresh()
        {
            var refreshToken = Request.Cookies[CookieNames.RefreshToken];

            var result = await _authService.RefreshSessionAsync(refreshToken);

            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            _refreshTokenCookieService.Set(result.Value.RefreshToken);

            var outputDto = new RefreshTokenOutputDto
            {
                AccessToken = result.Value.AccessToken,
            };

            return outputDto;
        }

        /// <summary>
        /// Logs out the current user and revokes the refresh token.
        /// </summary>
        /// <returns>A <see cref="NoContentResult"/> when logout is successful.</returns>
        /// <response code="204">The user was successfully logged out.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[CookieNames.RefreshToken];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken, "Logout");
                Response.Cookies.Delete(CookieNames.RefreshToken);
            }

            return NoContent();
        }
    }
}
