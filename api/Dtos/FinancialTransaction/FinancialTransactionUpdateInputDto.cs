using api.Enums;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents the data required to update an existing financial transaction.
    /// </summary>
    public record FinancialTransactionUpdateInputDto
    {
        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        required public decimal Amount { get; init; }

        /// <summary>
        /// Gets the comment or description associated with the transaction.
        /// </summary>
        [Required]
        [MaxLength(255, ErrorMessage = "Comment can not be over 255 characters")]
        required public string Comment { get; init; }

        /// <summary>
        /// Gets the type of the financial transaction.
        /// </summary>
        [EnumDataType(typeof(FinancialTransactionType))]
        required public FinancialTransactionType Type { get; init; }

        /// <summary>
        /// Gets the identifier of the category assigned to the transaction.
        /// </summary>
        required public Guid CategoryId { get; init; }
    }
}
