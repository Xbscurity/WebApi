using api.Enums;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents the data required to create a new financial transaction as an administrator.
    /// </summary>
    public record AdminFinancialTransactionCreateInputDto
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

        /// <summary>
        /// Gets the identifier of the target user for the transaction.
        /// </summary>
        /// <remarks>
        /// This field is optional and is typically used only by administrators
        /// to create transactions on behalf of another user.
        /// </remarks>
        required public string AppUserId { get; init; }
    }
}
