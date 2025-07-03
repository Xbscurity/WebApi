using System.ComponentModel.DataAnnotations;

namespace api.Dtos.FinancialTransactions
{
    public record BaseFinancialTransactionInputDto
    {
        [Required]
        public int CategoryId { get; init; }
        [Required]
        [Range(-100000000000, 100000000000)]
        public decimal Amount { get; init; }
        [Required]
        [MaxLength(255, ErrorMessage = "Comment can not be over 255 characters")]
        public string Comment { get; init; } = string.Empty;
    }
}