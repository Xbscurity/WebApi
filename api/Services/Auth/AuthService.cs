using api.Constants;
using api.Dtos.Account;
using api.Dtos.User;
using api.Extensions;
using api.Models;
using api.Repositories;
using api.Services.Categories;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.User;
using ErrorOr;

namespace api.Services.Auth
{
    /// <summary>
    /// Default implementation of <see cref="IAuthService"/>.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ITokenRepository _tokenRepository;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AuthService> _logger;
        private readonly IUnitOfWorkService _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userService">
        /// The service used for user management operations.
        /// </param>
        /// <param name="tokenService">
        /// The service used for token generation and validation.
        /// </param>
        /// <param name="tokenRepository">
        /// The repository used to manage refresh token persistence.
        /// </param>
        /// <param name="categoryService">
        /// The service used to create initial user categories.
        /// </param>
        /// <param name="logger">
        /// The logger used for diagnostic and security logging.
        /// </param>
        /// <param name="unitOfWork">
        /// The unit of work used to execute transactional operations.
        /// </param>
        public AuthService(
            IUserService userService,
            ITokenService tokenService,
            ITokenRepository tokenRepository,
            ICategoryService categoryService,
            ILogger<AuthService> logger,
            IUnitOfWorkService unitOfWork)
        {
            _userService = userService;
            _tokenService = tokenService;
            _tokenRepository = tokenRepository;
            _categoryService = categoryService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<AuthResult>> RegisterAsync(RegisterInputDto dto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync<AuthResult>(async () =>
            {
                var user = new AppUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                };

                var created = await _userService.CreateAsync(user, dto.Password);
                if (!created.Succeeded)
                {
                    var errors = created.ToErrorDictionary();

                    _logger.LogWarning(LoggingEvents.Auth.RegisterFailed, "Failed to create user: {@Errors}", errors);

                    return created.MapToErrors();
                }

                var roleResult = await _userService.AddToRoleAsync(user, Roles.User);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.ToErrorDictionary();

                    _logger.LogError(LoggingEvents.Auth.AssignRoleFailed, "Failed to assign role to user: {@Errors}", errors);

                    return roleResult.MapToErrors();
                }

                await _categoryService.CreateInitialCategoriesForUserAsync(user.Id);

                var result = await GenerateAuthResultAsync(user);

                return result;
            });
            }

        /// <inheritdoc/>
        public async Task<ErrorOr<AuthResult>> LoginAsync(LoginInputDto dto)
        {
            var user = await _userService.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.User.NotFound, "User not found");

                return Errors.Auth.InvalidCredentials();
            }

            if (user.IsBanned)
            {
                _logger.LogInformation("Banned user with id {userId} attempted to authenticate", user.Id);
                return Errors.User.Banned();
            }

            var valid = await _userService.CheckPasswordAsync(user, dto.Password);
            if (!valid)
            {
                _logger.LogWarning(LoggingEvents.Auth.InvalidCredentials, "Wrong password");
                return Errors.Auth.InvalidCredentials();
            }

            var result = await GenerateAuthResultAsync(user);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<RefreshTokenDto>> RefreshSessionAsync(string? refreshToken)
        {
            var validationResult = await _tokenService.ValidateStoredTokenAsync(refreshToken);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var storedToken = validationResult.Value;

            var user = await _userService.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return Errors.User.NotFound(storedToken.UserId);
            }

            if (user.IsBanned)
            {
                await _tokenRepository.RevokeAllRefreshTokensAsync(user.Id, "User banned");
                return Errors.User.Banned();
            }

            return await _unitOfWork.ExecuteInTransactionAsync<RefreshTokenDto>(async () =>
            {
                var rotationResult = await _tokenService.RotateTokensAsync(user, storedToken);
                if (rotationResult.IsError)
                {
                    return rotationResult.Errors;
                }

                return rotationResult.Value;
            });
        }

        /// <summary>
        /// Generates an authentication result for the specified user.
        /// </summary>
        /// <param name="user">
        /// The user for whom authentication tokens should be generated.
        /// </param>
        /// <returns>
        /// An authentication result containing user information,
        /// an access token, and a refresh token.
        /// </returns>
        private async Task<AuthResult> GenerateAuthResultAsync(AppUser user)
        {
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

            return new AuthResult
            {
                UserName = user.UserName!,
                Email = user.Email!,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
    }
}
