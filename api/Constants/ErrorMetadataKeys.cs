using ErrorOr;

namespace api.Constants
{
    /// <summary>
    /// Defines standardized keys used for attaching metadata to <see cref="Error"/> instances.
    /// </summary>
    public class ErrorMetadataKeys
    {
        /// <summary>
        /// Metadata key representing the value that caused the error.
        /// </summary>
        public const string Value = "value";

        /// <summary>
        /// Metadata key representing the field associated with the error.
        /// </summary>
        public const string Field = "field";

        /// <summary>
        /// Metadata key representing a collection of valid values or fields allowed by the validation logic.
        /// </summary>
        public const string AllowedFields = "allowedFields";
    }
}
