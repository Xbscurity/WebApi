namespace api.Validation
{
    /// <summary>
    /// Validates sort fields for <see cref="api.Models.Category"/> queries.
    /// </summary>
    public class CategorySortValidator : SortValidatorBase
    {
        /// <inheritdoc/>
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
        { "id", "name", "isactive" };
    }
}
