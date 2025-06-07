using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace api.QueryObjects
{
    public record PaginationQueryObject
    {
        [FromQuery(Name = "page")]
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [FromQuery(Name = "size")]
        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int Size { get; set; } = 10;

        [FromQuery(Name = "isDescending")]
        public bool IsDescending { get; set; } = false;

        [FromQuery(Name = "sortBy")]
        public string SortBy { get; set; } = "id";
    }
}
