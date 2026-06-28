using api.Constants;
using api.Dtos.Account;
using api.Dtos.User;
using api.Providers.ClientIpProvider;
using api.Providers.CurrentUser;
using api.Repositories;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.User;
using ErrorOr;

namespace api.Services.Account
{
    /// <summary>
    /// Default implementation of <see cref="IAccountService"/>.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly IClientIpProvider _clientIpProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWorkService _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="userService">
        /// The service used for user management operations.
        /// </param>
        /// <param name="tokenService">
        /// The service used for token generation operations.
        /// </param>
        /// <param name="tokenRepository">
        /// The repository used to manage refresh tokens.
        /// </param>
        /// <param name="logger">
        /// The logger used for diagnostic and security logging.
        /// </param>
        /// <param name="clientIpProvider">
        /// The provider used to retrieve the current client IP address.
        /// </param>
        /// <param name="currentUser">
        /// The current authenticated user context.
        /// </param>
        /// <param name="unitOfWork">
        /// The unit of work used to execute transactional operations.
        /// </param>
        public AccountService(
            IUserService userService,
            ITokenService tokenService,
            ITokenRepository tokenRepository,
            ILogger<AccountService> logger,
            IClientIpProvider clientIpProvider,
            ICurrentUser currentUser,
            IUnitOfWorkService unitOfWork)
        {
            _userService = userService;
            _tokenService = tokenService;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _clientIpProvider = clientIpProvider;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<UserProfileOutputDto>> GetProfileAsync()
        {
            var userId = _currentUser.UserId;
            var user = await _userService.FindByIdAsync(userId);

            var result = new UserProfileOutputDto
            {
                UserName = user!.UserName!,
                Email = user.Email!,
                CreatedAt = user.CreatedAt,
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordInputDto dto)
        {
            var userId = _currentUser.UserId;
            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                return Errors.User.NotFound(userId);
            }

            var passwordCheck = await _userService.CheckPasswordAsync(user, dto.CurrentPassword);

            if (!passwordCheck)
            {
                _logger.LogWarning(LoggingEvents.Auth.InvalidCredentials, "Invalid credentials");
                return Errors.Auth.InvalidCredentials();
            }

            return await _unitOfWork.ExecuteInTransactionAsync<string>(async () =>
            {
                var updateResult = await _userService.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (updateResult.IsError)
                {
                    return updateResult.Errors;
                }

                var ip = _clientIpProvider.GetClientIp();

                await _tokenRepository.RevokeAllRefreshTokensAsync(ip, "Password changed");

                var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

                return refreshToken;
            });
        }
    }
}
