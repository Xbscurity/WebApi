using api.Providers.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FinancialTransaction
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Amount { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public string AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        public FinancialTransaction(ITimeProvider timeProvider)
        {
            CreatedAt = timeProvider.UtcNow;
        }

        private FinancialTransaction()
        {
        }
    }
}