using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the data required to update an existing category.
    /// </summary>
    public record CategoryUpdateInputDto
    {
        /// <summary>
        /// Gets the new name of the category.
        /// </summary>
        /// <remarks>
        /// The name must be between 3 and 20 characters long.
        /// </remarks>
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Name can not be over 20 characters")]
        required public string Name { get; init; }
    }
}