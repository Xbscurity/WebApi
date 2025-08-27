namespace api.Validation
{
    public abstract class SortValidatorBase
    {
        protected abstract HashSet<string> ValidFields { get; }

        public bool IsValid(string? field) =>
            string.IsNullOrWhiteSpace(field) || ValidFields.Contains(field);

        public string GetErrorMessage(string field) =>
            $"SortBy '{field}' is not a valid field.";
    }
}
