namespace api.Validation
{
    /// <summary>
    /// Provides a base class for validating sort fields in API queries.
    /// </summary>
    /// <remarks>
    /// Derived classes define the set of valid sortable fields for specific entities.
    /// Used in controllers to ensure <c>SortBy</c> parameters are restricted to allowed values.
    /// </remarks>
    public abstract class SortValidatorBase
    {
        /// <summary>
        /// Gets the set of valid fields that can be used for sorting.
        /// </summary>
        protected abstract HashSet<string> ValidFields { get; }

        /// <summary>
        /// Determines whether the specified field is valid for sorting.
        /// </summary>
        /// <param name="field">The field name provided by the client.</param>
        /// <returns>
        /// <see langword="true"/> if the field is <see langword="null"/>, empty,
        /// or contained in <see cref="ValidFields"/>; otherwise <see langword="false"/>.
        /// </returns>
        public bool IsValid(string? field) =>
            string.IsNullOrWhiteSpace(field) || ValidFields.Contains(field);

        /// <summary>
        /// Returns a standardized error message for an invalid sort field.
        /// </summary>
        /// <param name="field">The invalid field name.</param>
        /// <returns>A formatted error message indicating the invalid field.</returns>
        public string GetErrorMessage(string field) =>
            $"SortBy '{field}' is not a valid field.";
    }
}
