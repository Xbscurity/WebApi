namespace api.Attributes
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Validates that the trimmed length of a string is within the specified range.
    /// </summary>
    /// <remarks>
    /// Leading and trailing whitespace is removed before the string length is evaluated.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TrimmedLengthAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrimmedLengthAttribute"/> class.
        /// </summary>
        /// <param name="minimumLength">
        /// The minimum allowed length of the trimmed string.
        /// </param>
        /// <param name="maximumLength">
        /// The maximum allowed length of the trimmed string.
        /// Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        public TrimmedLengthAttribute(int minimumLength, int maximumLength = int.MaxValue)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }

        /// <summary>
        /// Gets the minimum allowed length of the trimmed string.
        /// </summary>
        public int MinimumLength { get; }

        /// <summary>
        /// Gets the maximum allowed length of the trimmed string.
        /// </summary>
        public int MaximumLength { get; }

        /// <summary>
        /// Validates the trimmed length of the specified value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">
        /// The context in which the validation is performed.
        /// </param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> when the value is null,
        /// is not a string, or its trimmed length is within the specified range;
        /// otherwise, a <see cref="ValidationResult"/> containing the validation error.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is string str)
            {
                var trimmed = str.Trim();

                if (trimmed.Length < MinimumLength || trimmed.Length > MaximumLength)
                {
                    var memberNames = validationContext.MemberName is not null
                        ? new[] { validationContext.MemberName }
                        : null;

                    return new ValidationResult(
                        ErrorMessage ?? $"Length must be between {MinimumLength} and {MaximumLength} characters.",
                        memberNames);
                }
            }

            return ValidationResult.Success;
        }
    }
}