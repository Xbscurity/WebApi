namespace api.Validation
{
    /// <summary>
    /// Validates sort fields for <see cref="api.Models.FinancialTransaction"/> queries.
    /// </summary>
    public class FinancialTransactionSortValidator : SortValidatorBase
    {
        /// <inheritdoc/>
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
    { "id", "category", "amount", "date" };
    }
}
