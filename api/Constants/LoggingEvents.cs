namespace api.Constants
{
    /// <summary>
    /// Provides a centralized collection of structured <see cref="EventId"/> definitions used for application logging.
    /// </summary>
    public static class LoggingEvents
    {
        /// <summary>
        /// EventIds related to auth operations (1000–1499).
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Event ID for when user registration fails.
            /// </summary>
            public static readonly EventId RegisterFailed = new(1001, nameof(RegisterFailed));

            /// <summary>
            /// Event ID for when assigning a role to a user fails.
            /// </summary>
            public static readonly EventId AssignRoleFailed = new(1002, nameof(AssignRoleFailed));

            /// <summary>
            /// Event ID for when a login attempt is made with invalid credentials.
            /// </summary>
            public static readonly EventId InvalidCredentials = new(1003, nameof(InvalidCredentials));

            /// <summary>
            /// Event ID for when a password has been changed.
            /// </summary>
            public static readonly EventId PasswordChanged = new(1004, nameof(PasswordChanged));

            /// <summary>
            /// Event ID for when a password update operation fails.
            /// </summary>
            public static readonly EventId UpdatePasswordFailed = new(1005, nameof(UpdatePasswordFailed));

            /// <summary>
            /// Event ID for when an authenticated user attempts an action they are not permitted to perform.
            /// </summary>
            public static readonly EventId Forbidden = new(1006, nameof(Forbidden));

            /// <summary>
            /// EventIds related to refresh token operations (1100–1199).
            /// </summary>
            public static class RefreshToken
            {
                /// <summary>
                /// Event ID for when an error occurs during expired refresh token cleanup.
                /// </summary>
                public static readonly EventId CleanupError = new(1100, nameof(CleanupError));

                /// <summary>
                /// Event ID for when a refresh token references a user that no longer exists.
                /// </summary>
                public static readonly EventId UserMissing = new(1101, nameof(UserMissing));

                /// <summary>
                /// Event ID for when a refresh token that has already been used is presented again,
                /// indicating a possible token theft or replay attack.
                /// </summary>
                public static readonly EventId ReuseAttempt = new(1102, nameof(ReuseAttempt));

                /// <summary>
                /// Event ID for when the presented refresh token does not exist in the store.
                /// </summary>
                public static readonly EventId NotFound = new(1103, nameof(NotFound));

                /// <summary>
                /// Event ID for when no refresh token was supplied with the request.
                /// </summary>
                public static readonly EventId NotSupplied = new(1104, nameof(NotSupplied));
            }
        }

        /// <summary>
        /// EventIds related to user operations (1500–1999).
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Event ID for when an invalid sort parameter is provided.
            /// </summary>
            public static readonly EventId SortInvalid = new(1500, nameof(SortInvalid));

            /// <summary>
            /// Event ID for when a user update operation fails.
            /// </summary>
            public static readonly EventId UpdateFailed = new(1501, nameof(UpdateFailed));

            /// <summary>
            /// Event ID for when a requested user is not found.
            /// </summary>
            public static readonly EventId NotFound = new(1502, nameof(NotFound));

            /// <summary>
            /// Event ID for when an attempt is made to ban a user with the Admin role,
            /// which is a restricted operation.
            /// </summary>
            public static readonly EventId AdminBanAttempt = new(1503, nameof(AdminBanAttempt));
        }

        /// <summary>
        /// EventIds related to category operations (2000–2499).
        /// </summary>
        public static class Category
        {
            /// <summary>
            /// Event ID for when an invalid sort parameter is provided.
            /// </summary>
            public static readonly EventId SortInvalid = new(2000, nameof(SortInvalid));

            /// <summary>
            /// Event ID for when an unauthorized access attempt is made.
            /// </summary>
            public static readonly EventId AccessDenied = new(2001, nameof(AccessDenied));

            /// <summary>
            /// Event ID for when a category not found.
            /// </summary>
            public static readonly EventId NotFound = new(2002, nameof(NotFound));

            /// <summary>
            /// Event ID for when a category cannot be deleted because of existing related entities.
            /// </summary>
            public static readonly EventId DeleteRestricted = new(2003, nameof(DeleteRestricted));

            /// <summary>
            /// Event ID for when a category is created.
            /// </summary>
            public static readonly EventId Created = new(2004, nameof(Created));

            /// <summary>
            /// Event ID for when a category is updated.
            /// </summary>
            public static readonly EventId Updated = new(2005, nameof(Updated));

            /// <summary>
            /// Event ID for when a category's active state is toggled.
            /// </summary>
            public static readonly EventId Toggled = new(2006, nameof(Toggled));

            /// <summary>
            /// Event ID for when a category is deleted.
            /// </summary>
            public static readonly EventId Deleted = new(2007, nameof(Deleted));
        }

        /// <summary>
        ///  EventIds related to finanсical transactions operations (2500-2999).
        /// </summary>
        public static class FinancialTransaction
        {
            /// <summary>
            /// Event ID for when an invalid sort parameter is provided.
            /// </summary>
            public static readonly EventId SortInvalid = new(2500, nameof(SortInvalid));

            /// <summary>
            /// Event ID for when a financial transaction is not found.
            /// </summary>
            public static readonly EventId NotFound = new(2501, nameof(NotFound));

            /// <summary>
            /// Event ID for when an unauthorized access attempt is made.
            /// </summary>
            public static readonly EventId AccessDenied = new(2502, nameof(AccessDenied));

            /// <summary>
            /// Event ID for when a financial transaction is created.
            /// </summary>
            public static readonly EventId Created = new(2503, nameof(Created));

            /// <summary>
            /// Event ID for when a financial transaction is updated.
            /// </summary>
            public static readonly EventId Updated = new(2504, nameof(Updated));

            /// <summary>
            /// Event ID for when a financial transaction is deleted.
            /// </summary>
            public static readonly EventId Deleted = new(2505, nameof(Deleted));

            /// <summary>
            /// Event ID for when a grouping strategy that is not supported is requested.
            /// </summary>
            public static readonly EventId NotSupportedStrategyGrouping = new(2506, nameof(NotSupportedStrategyGrouping));
        }
    }
}