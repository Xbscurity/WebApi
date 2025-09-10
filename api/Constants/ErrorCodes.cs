namespace api.Constants
{
    /// <summary>
    /// Contains standard error codes used across the API.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Error code indicating that a requested resource was not found.
        /// </summary>
        public const string NotFound = "NOT_FOUND";

        /// <summary>
        /// Error code indicating that the request is invalid or malformed.
        /// </summary>
        public const string BadRequest = "BAD_REQUEST";

        /// <summary>
        /// Error code indicating that the user is not authenticated.
        /// </summary>
        public const string Unauthorized = "UNAUTHORIZED";

        /// <summary>
        /// Error code indicating that the user is authenticated but does not have permission.
        /// </summary>
        public const string Forbidden = "FORBIDDEN";

        /// <summary>
        /// Error code indicating that one or more validation errors occurred.
        /// </summary>
        public const string ValidationError = "VALIDATION_ERROR";

        public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
    }
}
