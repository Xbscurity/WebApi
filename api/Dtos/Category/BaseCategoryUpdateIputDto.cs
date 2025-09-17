using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the input DTO for updating an existing category.
    /// </summary>
    public record BaseCategoryUpdateInputDto
    {
        /// <summary>
        /// Gets the name of the category.
        /// <para>Must be between 3 and 20 characters.</para>
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Name can not be over 10 characters")]
        required public string Name { get; init; }
    }
}