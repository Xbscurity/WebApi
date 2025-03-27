using api.Services.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FinancialTransaction
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        private FinancialTransaction() { }
        public FinancialTransaction(ITimeProvider timeProvider)
        {
            CreatedAt = timeProvider.UtcNow;
        }
    }
}