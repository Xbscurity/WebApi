using api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the data required to create a new category as an administrator.
    /// </summary>
    public record AdminCategoryCreateInputDto
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <remarks>
        /// The name must be between 3 and 20 characters long.
        /// </remarks>
        [Required]
        [TrimmedLength(3, 20)]
        required public string Name { get; init; }

        /// <summary>
        /// Gets the identifier of the target user for the category to create.
        /// </summary>
        [Required]
        required public string AppUserId { get; init; }
    }
}