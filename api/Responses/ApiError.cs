namespace api.Responses
{
    /// <summary>
    /// Represents a standardized error response returned by the API.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Gets or sets a machine-readable error code.
        /// <para>
        /// The value should correspond to one of the constants defined in <see cref="Constants.ErrorCodes"/>.
        /// </para>
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Gets or sets a human-readable description of the error.
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        /// Gets or sets optional additional data associated with the error.
        /// For example, validation error details.
        /// </summary>
        public object? Data { get; set; }
    }
}
