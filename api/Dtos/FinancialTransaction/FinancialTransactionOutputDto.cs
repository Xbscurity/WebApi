namespace api.Dtos.FinancialTransactions
{
    public record FinancialTransactionOutputDto
    {
        public int Id { get; init; }
        public string? CategoryName { get; init; }
        public decimal Amount { get; init; }
        public string Comment { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
