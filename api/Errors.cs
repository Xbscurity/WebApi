using api.Constants;
using api.Services.FinancialTransactions;
using ErrorOr;

namespace api
{
    /// <summary>
    /// Centralized factory for application errors grouped by domain.
    /// Provides strongly-typed, consistent error creation.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Errors related to category operations.
        /// </summary>
        public static class Category
        {
            /// <summary>
            /// Prefix used for all category-related error codes.
            /// </summary>
            public const string Prefix = "CATEGORY_";

            /// <summary>
            /// Creates an error indicating that a category with the specified ID was not found.
            /// </summary>
            /// <param name="id">The category identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.NotFound"/>.</returns>
            public static Error NotFound(Guid id) =>
                Error.NotFound(
                    code: $"{Prefix}NOT_FOUND",
                    description: $"Category with id {id} was not found.",
                    metadata: new Dictionary<string, object>
                    {
                        {
                            "categoryId", id
                        },
                    });

            /// <summary>
            /// Creates an error indicating an invalid sort field for categories.
            /// </summary>
            /// <param name="name">The provided sort field.</param>
            /// <param name="allowedFields">A collection of allowed sort fields.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Validation"/>.</returns>
            public static Error InvalidSortBy(string name, IEnumerable<string> allowedFields) =>
                Error.Validation(
                    code: $"{Prefix}INVALID_SORT_BY",
                    description: $"{name} is not a valid sortBy value for category",
                    metadata: new Dictionary<string, object>
                    {
                        [ErrorMetadataKeys.Field] = "sortBy",
                        [ErrorMetadataKeys.Value] = name,
                        [ErrorMetadataKeys.AllowedFields] = allowedFields,
                    });

            /// <summary>
            /// Creates an error indicating that access to a category is forbidden.
            /// </summary>
            /// <param name="id">The category identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Forbidden"/>.</returns>
            public static Error AccessDenied(Guid id) =>
                Error.Forbidden(
                    code: $"{Prefix}ACCESS_DENIED",
                    description: $"An unauthorized access to category with id {id}",
                    metadata: new Dictionary<string, object>
                    {
                        {
                            "categoryId", id
                        },
                    });

            /// <summary>
            /// Creates an error indicating that a category cannot be deleted due to existing dependencies.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Conflict"/>.</returns>
            public static Error DeleteRestricted() =>
                Error.Conflict(
                    code: $"{Prefix}DELETE_RESTRICTED",
                    description: $"Category has existing related entities.");
        }

        /// <summary>
        /// Errors related to financial transactions.
        /// </summary>
        public static class FT
        {
            /// <summary>
            /// Prefix used for all financial transaction-related error codes.
            /// </summary>
            public const string Prefix = "FT_";

            /// <summary>
            /// Creates an error indicating that a financial transaction was not found.
            /// </summary>
            /// <param name="id">The transaction identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.NotFound"/>.</returns>
            public static Error NotFound(Guid id) =>
                Error.NotFound(
                    code: $"{Prefix}NOT_FOUND",
                    description: $"Financial transaction with id {id} was not found.",
                    metadata: new Dictionary<string, object>
                    {
                        {
                            "financialTransactionId", id
                        },
                    });

            /// <summary>
            /// Creates an error indicating an invalid sort field for financial transactions.
            /// </summary>
            /// <param name="sortBy">The provided sort field.</param>
            /// <param name="allowedFields">A collection of allowed sort fields.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Validation"/>.</returns>
            public static Error InvalidSortBy(string sortBy, IEnumerable<string> allowedFields) =>
                Error.Validation(
                    code: $"{Prefix}INVALID_SORT_BY",
                    description: $"{sortBy} is not a valid sortBy value for financial transaction",
                    metadata: new Dictionary<string, object>
                    {
                        [ErrorMetadataKeys.Field] = "sortBy",
                        [ErrorMetadataKeys.Value] = sortBy,
                        [ErrorMetadataKeys.AllowedFields] = allowedFields,
                    });

            /// <summary>
            /// Creates an error indicating that access to a financial transaction is forbidden.
            /// </summary>
            /// <param name="id">The transaction identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Forbidden"/>.</returns>
            public static Error AccessDenied(Guid id) =>
                Error.Forbidden(
                    code: $"{Prefix}ACCESS_DENIED",
                    description: $"An unauthorized access to financial transaction with id {id}",
                    metadata: new Dictionary<string, object>
                    {
                        {
                            "financialTransactionId", id
                        },
                    });

            /// <summary>
            /// Creates an error indicating that the provided grouping strategy is not supported.
            /// </summary>
            /// <param name="name">The strategy key.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Validation"/>.</returns>
            public static Error UnsupportedStrategy(GroupingReportStrategyKey name) =>
                Error.Validation(
                    code: $"{Prefix}UNSUPPORTED_STRATEGY",
                    description: $"{name} is invalid grouping strategy",
                    metadata: new Dictionary<string, object>
                    {
                        [ErrorMetadataKeys.Field] = "key",
                        [ErrorMetadataKeys.Value] = name,
                    });
        }

        /// <summary>
        /// Errors related to user operations.
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Prefix used for all user-related error codes.
            /// </summary>
            public const string Prefix = "USER_";

            /// <summary>
            /// Creates an error indicating an invalid sort field for users.
            /// </summary>
            /// <param name="sortBy">The provided sort field.</param>
            /// <param name="allowedFields">A collection of allowed sort fields.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Validation"/>.</returns>
            public static Error InvalidSortBy(string sortBy, IEnumerable<string> allowedFields) =>
                Error.Validation(
                    code: $"{Prefix}INVALID_SORT_BY",
                    description: $"{sortBy} is invalid sort by.",
                    metadata: new Dictionary<string, object>
                    {
                        [ErrorMetadataKeys.Field] = "sortBy",
                        [ErrorMetadataKeys.Value] = sortBy,
                        [ErrorMetadataKeys.AllowedFields] = allowedFields,
                    });

            /// <summary>
            /// Creates an error indicating that a user was not found.
            /// </summary>
            /// <param name="id">The user identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.NotFound"/>.</returns>
            public static Error NotFound(string id) =>
                Error.NotFound(
                    code: $"{Prefix}NOT_FOUND",
                    description: $"User with id {id} not found.",
                    metadata: new Dictionary<string, object>
                    {
                        {
                            "userId", id
                        },
                    });

            /// <summary>
            /// Creates an error indicating that the user is banned.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Forbidden"/>.</returns>
            public static Error Banned() =>
                Error.Forbidden(
                    code: $"{Prefix}BANNED",
                    description: $"User is banned.");

            /// <summary>
            /// Creates an error indicating an attempt to ban an administrator.
            /// </summary>
            /// <param name="id">The admin identifier.</param>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Conflict"/>.</returns>
            public static Error AdminBanAttempt(string id) =>
                Error.Forbidden(
                    code: $"{Prefix}ADMIN_BAN_ATTEMPT",
                    description: $"Attempt to ban an admin with id {id}.");
        }

        /// <summary>
        /// Errors related to authentication and authorization.
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Prefix used for authentication-related error codes.
            /// </summary>
            public const string Prefix = "AUTH_";

            /// <summary>
            /// Prefix used for refresh token-related error codes.
            /// </summary>
            public const string Refresh = "REFRESH_";

            /// <summary>
            /// Creates an error indicating invalid authentication credentials.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Unauthorized"/>.</returns>
            public static Error InvalidCredentials() =>
                Error.Unauthorized(
                    code: $"{Prefix}INVALID_CREDENTIALS",
                    description: "Invalid username or password");

            /// <summary>
            /// Creates an error indicating that a refresh token was not provided.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Unauthorized"/>.</returns>
            public static Error RefreshTokenNotSupplied() =>
                Error.Unauthorized(
                    code: $"{Prefix}{Refresh}NOT_SUPPLIED",
                    description: "Refresh token not supplied in cookies");

            /// <summary>
            /// Creates an error indicating that a refresh token was not found.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Unauthorized"/>.</returns>
            public static Error RefreshTokenNotFound() =>
                Error.Unauthorized(
                    code: $"{Prefix}{Refresh}NOT_FOUND",
                    description: "Refresh token not found in cookies");

            /// <summary>
            /// Creates an error indicating that a refresh token has already been revoked.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Conflict"/>.</returns>
            public static Error RefreshTokenAlreadyRevoked() =>
                Error.Conflict(
                    code: $"{Prefix}{Refresh}ALREADY_REVOKED",
                    description: "Refresh token has been already revoked");

            /// <summary>
            /// Creates an error indicating that a refresh token has expired.
            /// </summary>
            /// <returns>A <see cref="Error"/> of type <see cref="ErrorType.Unauthorized"/>.</returns>
            public static Error RefreshTokenExpired() =>
                Error.Unauthorized(
                    code: $"{Prefix}{Refresh}EXPIRED",
                    description: "Refresh token has been expired");
        }
    }
}
