namespace api.Helpers
{
    public class PagedData<T>
    {
        public List<T> Data { get; set; }

        public Pagination Pagination { get; set; }
    }
}
