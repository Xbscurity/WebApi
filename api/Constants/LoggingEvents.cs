namespace api.Constants
{
    /// <summary>
    /// Contains centralized <see cref="EventId"/> definitions for logging throughout the application.
    /// Each ID provides a unique identifier for a specific type of event,
    /// making it easier to filter, search, and analyze logs.
    /// </summary>
    public static class LoggingEvents
    {
        /// <summary>
        /// EventIds related to user operations (1000–1499).
        /// </summary>
        public static class Users
        {
            /// <summary>
            /// AccountController (1000–1099).
            /// </summary>
            public static class Common
            {
                /// <summary>
                /// Event ID for user registration.
                /// </summary>
                public static readonly EventId Register = new(1001, nameof(Register));

                /// <summary>
                /// Event ID for user login.
                /// </summary>
                public static readonly EventId Login = new(1011, nameof(Login));

                /// <summary>
                /// Event ID for refresh token operations.
                /// </summary>
                public static readonly EventId RefreshToken = new(1021, nameof(RefreshToken));

                /// <summary>
                /// Event ID for password change operations.
                /// </summary>
                public static readonly EventId ChangePassword = new(1051, nameof(ChangePassword));
            }

            /// <summary>
            /// AdminUserManagementController (1100-1199).
            /// </summary>
            public static class Admin
            {
                /// <summary>
                /// Event ID for when a user is not found by the admin management controller.
                /// </summary>
                public static readonly EventId NotFound = new(1100, nameof(NotFound));

                /// <summary>
                /// Event ID for when an admin attempts to ban another administrator.
                /// </summary>
                public static readonly EventId AdminBanAttempt = new(1101, nameof(AdminBanAttempt));

                /// <summary>
                /// Event ID for user ban and unban operations.
                /// </summary>
                public static readonly EventId BanUser = new(1102, nameof(BanUser));

                /// <summary>
                /// Event ID for when an admin successfully retrieves all users.
                /// </summary>
                public static readonly EventId GetAll = new(1103, nameof(GetAll));
            }
        }

        /// <summary>
        /// EventIds related to category operations (1500–1999).
        /// </summary>
        public static class Categories
        {
            /// <summary>
            /// BaseCategoryController (1500–1599).
            /// </summary>
            public static class Common
            {
                /// <summary>
                /// Event ID for when an invalid sort parameter is provided.
                /// </summary>
                public static readonly EventId SortInvalid = new(1500, nameof(SortInvalid));

                /// <summary>
                /// Event ID for when an unauthorized access attempt is made.
                /// </summary>
                public static readonly EventId NoAccess = new(1501, nameof(NoAccess));

                /// <summary>
                /// Event ID for when a category is retrieved.
                /// </summary>
                public static readonly EventId GetById = new(1502, nameof(GetById));

                /// <summary>
                /// Event ID for when a category is deleted.
                /// </summary>
                public static readonly EventId Deleted = new(1503, nameof(Deleted));

                /// <summary>
                /// Event ID for when a category is updated.
                /// </summary>
                public static readonly EventId Updated = new(1504, nameof(Updated));

                /// <summary>
                /// Event ID for when a category's active state is toggled.
                /// </summary>
                public static readonly EventId Toggled = new(1505, nameof(Toggled));
            }

            /// <summary>
            /// AdminCategoryController (1600–1699).
            /// </summary>
            public static class Admin
            {
                /// <summary>
                /// Event ID for retrieving all categories by an admin.
                /// </summary>
                public static readonly EventId GetAll = new(1600, nameof(GetAll));

                /// <summary>
                /// Event ID for when an admin creates a category.
                /// </summary>
                public static readonly EventId Created = new(1601, nameof(Created));
            }

            /// <summary>
            /// UserCategoryController (1700–1799).
            /// </summary>
            public static class User
            {
                /// <summary>
                /// Event ID for retrieving all category by a regular user.
                /// </summary>
                public static readonly EventId GetAll = new(1700, nameof(GetAll));

                /// <summary>
                /// Event ID for when a regular user creates a category.
                /// </summary>
                public static readonly EventId Created = new(1701, nameof(Created));
            }
        }

        /// <summary>
        ///  EventIds related to finanсical transactions operations (2000-2499).
        /// </summary>
        public static class FinancialTransactions
        {
            /// <summary>
            /// BaseTransactionController (2001–2099).
            /// </summary>
            public static class Common
            {
                /// <summary>
                /// Event ID for when an invalid sort parameter is provided.
                /// </summary>
                public static readonly EventId SortInvalid = new(2000, nameof(SortInvalid));

                /// <summary>
                /// Event ID for when an unauthorized access attempt is made.
                /// </summary>
                public static readonly EventId NoAccess = new(2001, nameof(NoAccess));

                /// <summary>
                /// Event ID for when a financial transaction is retrieved by ID.
                /// </summary>
                public static readonly EventId GetById = new(2002, nameof(GetById));

                /// <summary>
                /// Event ID for when a financial transaction is deleted.
                /// </summary>
                public static readonly EventId Deleted = new(2003, nameof(Deleted));

                /// <summary>
                /// Event ID for when a financial transaction is updated.
                /// </summary>
                public static readonly EventId Updated = new(2004, nameof(Updated));

                /// <summary>
                /// Event ID for when a financial transaction's active state is toggled.
                /// </summary>
                public static readonly EventId Toggled = new(2005, nameof(Toggled));
            }

            /// <summary>
            /// AdminTransactionController (2100–2199).
            /// </summary>
            public static class Admin
            {
                /// <summary>
                /// Event ID for retrieving all transactions by an admin.
                /// </summary>
                public static readonly EventId GetAll = new(2100, nameof(GetAll));

                /// <summary>
                /// Event ID for when an admin creates a transaction.
                /// </summary>
                public static readonly EventId Created = new(2101, nameof(Created));
            }

            /// <summary>
            /// UserTransactionController (2200–2299).
            /// </summary>
            public static class User
            {
                /// <summary>
                /// Event ID for retrieving all transactions by a regular user.
                /// </summary>
                public static readonly EventId GetAll = new(2200, nameof(GetAll));

                /// <summary>
                /// Event ID for when a regular user creates a transaction.
                /// </summary>
                public static readonly EventId Created = new(2201, nameof(Created));

                /// <summary>
                /// Event ID for when a user generates a financial report.
                /// </summary>
                public static readonly EventId Report = new(2202, nameof(Report));
            }
        }
    }
}
