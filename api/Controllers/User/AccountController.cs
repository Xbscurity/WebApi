using api.Constants;
using api.Dtos.Account;
using api.Dtos.User;
using api.Services.Account;
using api.Services.RefreshTokenCookie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace api.Controllers.User
{
    /// <summary>
    /// Provides endpoints for managing the authenticated user's account.
    /// </summary>
    /// <remarks>
    /// All endpoints require authentication and are accessible only to users
    /// who satisfy the <c>NotBanned</c> authorization policy.
    /// </remarks>
    [ApiController]
    [Authorize(Policy = Policies.NotBanned)]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IRefreshTokenCookieService _refreshTokenCookieService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="accountService">
        /// The service responsible for account-related operations.
        /// </param>
        /// <param name="refreshTokenCookieService">
        /// The service responsible for managing refresh token cookies.
        /// </param>
        public AccountController(
            IAccountService accountService,
            IRefreshTokenCookieService refreshTokenCookieService)
        {
            _accountService = accountService;
            _refreshTokenCookieService = refreshTokenCookieService;
        }

        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// The authenticated user's profile information.
        /// </returns>
        /// <response code="200">
        /// Returns the authenticated user's profile information.
        /// </response>
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileOutputDto>> GetProfile()
        {
            var result = await _accountService.GetProfileAsync();
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="dto">
        /// The password change request containing the current and new passwords.
        /// </param>
        /// <returns>
        /// A <see cref="NoContentResult"/> when the password is successfully changed.
        /// </returns>
        /// <remarks>
        /// After a successful password change, all existing refresh tokens are revoked
        /// and a new refresh token is issued and stored in an HTTP-only cookie.
        /// </remarks>
        /// <response code="204">
        /// The password was successfully changed.
        /// </response>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordInputDto dto)
        {
            var result = await _accountService.ChangePasswordAsync(dto);

            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            _refreshTokenCookieService.Set(result.Value);

            return NoContent();
        }
    }
}
