using api.Attributes;
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
        [TrimmedLength(1, 255)]
        required public string Comment { get; init; }

        /// <summary>
        /// Gets the type of the financial transaction.
        /// </summary>
        required public FinancialTransactionType Type { get; init; }

        /// <summary>
        /// Gets the identifier of the category assigned to the transaction.
        /// </summary>
        required public Guid CategoryId { get; init; }
    }
}
