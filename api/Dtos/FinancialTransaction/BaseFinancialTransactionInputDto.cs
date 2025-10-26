using api.Dtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents the input DTO for creating or updating a financial transaction.
    /// </summary>
    public record BaseFinancialTransactionInputDto : IHasCategoryId
    {
        /// <summary>
        /// Gets the identifier of the category that the transaction belongs to.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public int CategoryId { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// <para>Can be positive (income) or negative (expense).</para>
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        [Range(-100000000000, 100000000000)]
        required public decimal Amount { get; init; }

        /// <summary>
        /// Gets the optional comment describing the transaction.
        /// <para>Maximum length is 255 characters.</para>
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        [MaxLength(255, ErrorMessage = "Comment can not be over 255 characters")]
        required public string Comment { get; init; }
    }
}