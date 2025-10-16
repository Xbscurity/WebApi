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

        /// <summary>
        /// Policy that allows access to categories.
        /// </summary>
        public const string CategoryAccess = nameof(CategoryAccess);

        /// <summary>
        /// Policy that ensures users can only access their own financial transactions,
        /// unless they are administrators.
        /// </summary>
        public const string TransactionAccess = nameof(TransactionAccess);
    }
}
