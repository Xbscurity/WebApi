namespace api.Constants
{
    /// <summary>
    /// Contains role names used for authorization in the application.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Standard user role with limited permissions.
        /// </summary>
        public const string User = nameof(User);

        /// <summary>
        /// Administrator role with elevated permissions.
        /// </summary>
        public const string Admin = nameof(Admin);
    }
}
