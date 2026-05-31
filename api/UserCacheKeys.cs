namespace api
{
    /// <summary>
    /// Provides cache key factories for user-related cache entries.
    /// </summary>
    /// <remarks>
    /// This class centralizes cache key generation to ensure
    /// consistent cache naming conventions across the application.
    /// </remarks>
    public static class UserCacheKeys
    {
        /// <summary>
        /// Creates a cache key for storing a user's ban status.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user.
        /// </param>
        /// <returns>
        /// A cache key representing the user's ban status entry.
        /// </returns>
        public static string BanStatus(string userId)
            => $"users:ban:{userId}";
    }
}
