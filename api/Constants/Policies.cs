namespace api.Constants
{
    /// <summary>
    /// Contains names of authorization policies used throughout the API.
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy that ensures the user is not banned.
        /// </summary>
        public const string UserNotBanned = nameof(UserNotBanned);

        /// <summary>
        /// Policy that restricts access to administrators only.
        /// </summary>
        public const string Admin = nameof(Admin);

        public const string CategoryAccessGlobal = nameof(CategoryAccessGlobal);

        public const string CategoryAccessNoGlobal = nameof(CategoryAccessNoGlobal);

        public const string TransactionAccess = nameof(TransactionAccess);
    }
}
