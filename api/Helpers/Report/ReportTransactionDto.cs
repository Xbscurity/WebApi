namespace api.Helpers
{
    public class ReportTransactionDto
    {
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
    }
}