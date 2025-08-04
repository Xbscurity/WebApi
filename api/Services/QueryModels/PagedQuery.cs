using api.Responses;

namespace api.Services.QueryModels
{
    public class PagedQuery<T>
    {
        public IQueryable<T> Query { get; set; }

        public Pagination Pagination { get; set; }
    }
}
