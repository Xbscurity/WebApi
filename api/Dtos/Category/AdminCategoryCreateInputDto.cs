using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the input DTO for creating a category by an administrator.
    /// </summary>
    public record AdminCategoryCreateInputDto
    {
        /// <summary>
        /// Gets the name of the category.
        /// This property is required and must be between 3 and 20 characters long.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Name can not be over 10 characters")]
        required public string Name { get; init; }

        /// <summary>
        /// Gets the ID of the user to whom this category belongs.
        /// <see langword="Null"/> indicates a global/common category.
        /// </summary>
        public string? AppUserId { get; init; }
    }
}
