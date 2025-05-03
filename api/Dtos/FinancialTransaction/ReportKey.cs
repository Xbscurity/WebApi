namespace api.Dtos.FinancialTransaction
{
    public record ReportKey
    {
        public string? Category { get; init; }
        public int? Year { get; init; }
        public int? Month { get; init; }

    }
}