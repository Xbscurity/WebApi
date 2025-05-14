namespace api.Helpers
{
    public class PagedQuery<T>
    {
        public IQueryable<T> Query { get; set; }
        public Pagination Pagination { get; set; }
    }
}
