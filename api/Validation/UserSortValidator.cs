namespace api.Validation
{
    public class UserSortValidator : SortValidatorBase
    {
        protected override HashSet<string> ValidFields { get; } = new(StringComparer.OrdinalIgnoreCase)
    { "id", "username", "email", "isbanned" };
    }
}
