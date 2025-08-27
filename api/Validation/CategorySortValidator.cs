namespace api.Validation
{
    public class CategorySortValidator : SortValidatorBase
    {
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
        { "id", "name", "isactive" };
    }
}
