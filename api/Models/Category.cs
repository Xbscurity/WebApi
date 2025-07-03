namespace api.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        public bool IsActive { get; set; } = true;
    }
}