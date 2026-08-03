using api.Constants;
using api.Dtos.User;
using api.Extensions;
using api.Models;
using api.Options;
using api.Providers.CurrentUser;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace api.Services.User
{
    /// <summary>
    /// Default implementation of <see cref="IUserService"/>.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IFusionCache _cache;
        private readonly CacheOptions _cacheOptions;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="cache">The memory cache service for ban status.</param>
        /// <param name="currentUser">Service providing information about the currently logged-in user.</param>
        /// <param name="cacheOptions">Configuration options for caching behavior.</param>
        /// <param name="logger">Logger for diagnostic information (e.g., cache hits/misses).</param>
        public UserService(
            UserManager<AppUser> userManager,
            IFusionCache cache,
            ICurrentUser currentUser,
            IOptions<CacheOptions> cacheOptions,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _currentUser = currentUser;
            _cacheOptions = cacheOptions.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<UserManagementUserOutputDto>> GetAllAsync(ISpecification<AppUser, UserManagementUserOutputDto> specification)
        {
            return await _userManager.Users
                .WithSpecification(specification)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<AppUser?> FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task<AppUser?> FindByNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(string id)
        {
            return await _userManager.Users.AnyAsync(u => u.Id == id);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        /// <inheritdoc />
        public async Task<IList<string>> GetRolesAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        /// <inheritdoc />
        public async ValueTask<bool> IsBannedAsync()
        {
            var cacheKey = UserCacheKeys.BanStatus(_currentUser.UserId);

            return await _cache.GetOrSetAsync(
                cacheKey,
                async _ =>
                {
                    var user = await _userManager.Users
                        .Where(u => u.Id == _currentUser.UserId)
                        .Select(u => new { u.IsBanned })
                        .SingleOrDefaultAsync();

                    return user?.IsBanned ?? true;
                },
                options => options.SetDuration(_cacheOptions.BanUserTtl));
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Created>> CreateAsync(AppUser user, string password)
        {
            var identityResult = await _userManager.CreateAsync(user, password);
            if (!identityResult.Succeeded)
            {
                var errors = identityResult.ToErrorDictionary();

                _logger.LogWarning(LoggingEvents.Auth.RegisterFailed, "Failed to create user: {@Errors}", errors);

                return identityResult.MapToErrors();
            }

            return Result.Created;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Updated>> UpdateAsync(AppUser user)
        {
            var identityResult = await _userManager.UpdateAsync(user);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.ToErrorDictionary();

                _logger.LogWarning(LoggingEvents.User.UpdateFailed, "User update failed: {@Errors}", errors);

                return identityResult.MapToErrors();
            }

            return Result.Updated;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Success>> AddToRoleAsync(AppUser user, string role)
        {
            var identityResult = await _userManager.AddToRoleAsync(user, role);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.ToErrorDictionary();

                _logger.LogError(LoggingEvents.Auth.AssignRoleFailed, "Failed to assign role to user: {@Errors}", errors);

                return identityResult.MapToErrors();
            }

            return Result.Success;
        }

        /// <inheritdoc />
        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Updated>> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {
            var identityResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.ToErrorDictionary();

                _logger.LogError(LoggingEvents.Auth.UpdatePasswordFailed, "Failed to change password: {@Errors}", errors);

                return identityResult.MapToErrors();
            }

            return Result.Updated;
        }
    }
}