using api.Dtos.FinancialTransactions;

namespace api.Dtos.FinancialTransaction
{
    public record GroupedReportDto
    {
        public ReportKey Key { get; init; }
        public IReadOnlyList<BaseFinancialTransactionOutputDto> Transactions { get; init; } = new List<BaseFinancialTransactionOutputDto>();
    }
}