using api.Dtos.User;
using api.Models;
using api.Options;
using api.Providers.CurrentUser;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace api.Services.User
{
    /// <summary>
    /// Default implementation of <see cref="IUserService"/>.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="memoryCache">The memory cache service for ban status.</param>
        /// <param name="currentUser">Service providing information about the currently logged-in user.</param>
        /// <param name="cacheOptions">Configuration options for caching behavior.</param>
        /// <param name="logger">Logger for diagnostic information (e.g., cache hits/misses).</param>
        public UserService(
            UserManager<AppUser> userManager,
            IMemoryCache memoryCache,
            ICurrentUser currentUser,
            IOptions<CacheOptions> cacheOptions,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _memoryCache = memoryCache;
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
        public ValueTask<bool> IsBannedAsync()
        {
            var cacheKey = UserCacheKeys.BanStatus(_currentUser.UserId);

            if (_memoryCache.TryGetValue(cacheKey, out bool isBanned))
            {
                _logger.LogInformation("Cache HIT for key: {CacheKey}", cacheKey);
                return new ValueTask<bool>(isBanned);
            }

            _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);
            return new ValueTask<bool>(LoadFromDbAndCacheAsync(cacheKey));

            async Task<bool> LoadFromDbAndCacheAsync(string cacheKey)
            {
                var user = await _userManager.Users
                    .Where(u => u.Id == _currentUser.UserId)
                    .Select(u => new { u.IsBanned })
                    .SingleOrDefaultAsync();

                bool isBanned = user?.IsBanned ?? true;

                _memoryCache.Set(cacheKey, isBanned, _cacheOptions.BanUserTtl);

                return isBanned;
            }
        }

        /// <inheritdoc />
        public async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> AddToRoleAsync(AppUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        /// <inheritdoc />
        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
    }
}