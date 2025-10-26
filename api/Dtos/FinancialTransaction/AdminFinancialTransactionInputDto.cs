using api.Dtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents the input DTO for a financial transaction for an administrator.
    /// </summary>
    public record AdminFinancialTransactionInputDto : IHasCategoryId
    {
        /// <summary>
        /// Gets the ID of the category associated with the transaction.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public int CategoryId { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// <para>Must be between -100,000,000,000 and 100,000,000,000.</para>
        /// <para>Can be positive (income) or negative (expense).</para>
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        [Range(-100000000000, 100000000000)]
        required public decimal Amount { get; init; }

        /// <summary>
        /// Gets a comment for the transaction.
        /// <para>Maximum length is 255 characters.</para>
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        [MaxLength(255, ErrorMessage = "Comment can not be over 255 characters")]
        required public string Comment { get; init; }

        /// <summary>
        /// Gets the ID of the user for whom the transaction is created.
        /// <para>This property is required.</para>
        /// </summary>
        required public string AppUserId { get; init; }
    }
}
