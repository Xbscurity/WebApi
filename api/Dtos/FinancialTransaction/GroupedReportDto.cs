using api.Dtos.FinancialTransactions;

namespace api.Dtos.FinancialTransaction
{
    public record GroupedReportDto
    {
        public ReportKey Key { get; init; }
        public IReadOnlyList<FinancialTransactionOutputDto> Transactions { get; init; } = new List<FinancialTransactionOutputDto>();
    }
}