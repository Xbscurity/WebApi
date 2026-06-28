namespace api.Constants
{
    /// <summary>
    /// Defines application role constants used for authorization.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Standard application user role.
        /// </summary>
        public const string User = nameof(User);

        /// <summary>
        /// Administrator role with elevated permissions.
        /// </summary>
        public const string Admin = nameof(Admin);
    }
}
