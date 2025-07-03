namespace api.Dtos.Category
{
    public class BaseCategoryOutputDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? AppUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
