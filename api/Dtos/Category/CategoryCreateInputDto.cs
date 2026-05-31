using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the data required to create a new category.
    /// </summary>
    public class CategoryCreateInputDto
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <remarks>
        /// The name must be between 3 and 20 characters long.
        /// </remarks>
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Name can not be over 20 characters")]
        required public string Name { get; init; }

        /// <summary>
        /// Gets the identifier of the target user for the category.
        /// </summary>
        /// <remarks>
        /// This field is optional and is typically used only by administrators
        /// to create categories on behalf of another user.
        /// </remarks>
        public string? TargetUserId { get; init; }
    }
}
