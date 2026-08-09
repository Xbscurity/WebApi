using api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the data required to create a new category.
    /// </summary>
    public record CategoryCreateInputDto
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
    }
}