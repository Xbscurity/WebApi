using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Category
{
    public record AdminCategoryInputDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Name can not be over 10 characters")]
        public string Name { get; init; }
        public string? AppUserId { get; init; }
    }
}
