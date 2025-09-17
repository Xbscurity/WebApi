namespace api.Validation
{
    /// <summary>
    /// Validates sort fields for <see cref="api.Models.AppUser"/> queries.
    /// </summary>
    public class UserSortValidator : SortValidatorBase
    {
        /// <inheritdoc/>
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
    { "id", "username", "email", "isbanned" };
    }
}
