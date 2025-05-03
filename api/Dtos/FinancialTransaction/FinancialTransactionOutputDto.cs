namespace api.Dtos.FinancialTransactions
{
    public record FinancialTransactionOutputDto(
        int Id, 
        string? CategoryName,
        decimal Amount,
        string Comment,
        DateTimeOffset CreatedAt
        );
}
