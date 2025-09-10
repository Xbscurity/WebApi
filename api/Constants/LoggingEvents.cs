namespace api.Constants
{
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
                public static readonly EventId RegisterFailed = new(1001, nameof(RegisterFailed));
                public static readonly EventId RoleAssignFailed = new(1002, nameof(RoleAssignFailed));

                public static readonly EventId LoginUserNotFound = new(1011, nameof(LoginUserNotFound));
                public static readonly EventId LoginInvalidPassword = new(1012, nameof(LoginInvalidPassword));

                public static readonly EventId RefreshTokenMissing = new(1021, nameof(RefreshTokenMissing));
                public static readonly EventId RefreshTokenInvalid = new(1022, nameof(RefreshTokenInvalid));

                public static readonly EventId ChangePasswordCurrentFailed = new(1051, nameof(ChangePasswordCurrentFailed));
                public static readonly EventId ChangePasswordNewFailed = new(1052, nameof(ChangePasswordNewFailed));
            }

            /// <summary>
            /// AdminUserManagementController (1100-1199).
            /// </summary>
            public static class Admin
            {
                public static readonly EventId NotFound = new(1100, nameof(NotFound));
                public static readonly EventId AdminBanAttempt = new(1101, nameof(AdminBanAttempt));
                public static readonly EventId UserAlreadyBanned = new(1102, nameof(UserAlreadyBanned));
                public static readonly EventId UserBanned = new(1103, nameof(UserBanned));
                public static readonly EventId GetAll = new(1104, nameof(GetAll));
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
                public static readonly EventId SortInvalid = new(1500, nameof(SortInvalid));
                public static readonly EventId NoAccess = new(1501, nameof(NoAccess));
                public static readonly EventId GetById = new(1502, nameof(GetById));
                public static readonly EventId Deleted = new(1503, nameof(Deleted));
                public static readonly EventId Updated = new(1504, nameof(Updated));
                public static readonly EventId Toggled = new(1505, nameof(Toggled));
            }

            /// <summary>
            /// AdminCategoryController (1600–1699).
            /// </summary>
            public static class Admin
            {
                public static readonly EventId GetAll = new(1600, nameof(GetAll));
                public static readonly EventId Created = new(1601, nameof(Created));
            }

            /// <summary>
            /// UserCategoryController (1700–1799).
            /// </summary>
            public static class User
            {
                public static readonly EventId GetAll = new(1700, nameof(GetAll));
                public static readonly EventId Created = new(1701, nameof(Created));
            }
        }

        /// <summary>
        ///  EventIds related to finanсical transactions operations (2000-2499).
        /// </summary>
        public static class Transactions
        {
            /// <summary>
            /// BaseTransactionController (2001–2099).
            /// </summary>
            public static class Common
            {
                public static readonly EventId SortInvalid = new(2000, nameof(SortInvalid));
                public static readonly EventId NoAccess = new(2001, nameof(NoAccess));
                public static readonly EventId GetById = new(2002, nameof(GetById));
                public static readonly EventId Deleted = new(2003, nameof(Deleted));
                public static readonly EventId Updated = new(2004, nameof(Updated));
                public static readonly EventId Toggled = new(2005, nameof(Toggled));
            }

            /// <summary>
            /// AdminTransactionController (2100–2199).
            /// </summary>
            public static class Admin
            {
                public static readonly EventId GetAll = new(2100, nameof(GetAll));
                public static readonly EventId Created = new(2101, nameof(Created));
            }

            /// <summary>
            /// UserTransactionController (2200–2299).
            /// </summary>
            public static class User
            {
                public static readonly EventId GetAll = new(2200, nameof(GetAll));
                public static readonly EventId Created = new(2201, nameof(Created));
                public static readonly EventId Report = new(2202, nameof(Report));
            }
        }
    }
}
