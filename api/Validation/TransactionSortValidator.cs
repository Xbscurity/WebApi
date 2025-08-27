namespace api.Validation
{
    public class TransactionSortValidator : SortValidatorBase
    {
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
    { "id", "category", "amount", "date" };
    }
}
