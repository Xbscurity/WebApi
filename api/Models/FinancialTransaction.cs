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
        public DateTime Date { get; set; } = DateTime.Now;
    }
}