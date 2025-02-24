using System.ComponentModel.DataAnnotations;

namespace api.Dtos
{
    public class FinancialTransactionDto
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [Range(-100000000000, 100000000000)]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Comment can not be over 255 characters")]
        public string Comment { get; set; } = string.Empty;
    }
}