namespace api.Helpers.Report
{
    public class GroupedReportDto
    {
        public ReportKey Key { get; set; }
        public List<ReportTransactionDto> Transactions { get; set; }
    }
}