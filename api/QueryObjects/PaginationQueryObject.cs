using System.ComponentModel.DataAnnotations;

namespace api.QueryObjects
{
    public record PaginationQueryObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int Size { get; set; } = 10;

        public bool IsDescending { get; set; } = false;

        public string SortBy { get; set; } = "Id";
    }
}
