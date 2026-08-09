using api.Attributes;
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
        [TrimmedLength(3, 20)]
        required public string Name { get; init; }
    }
}