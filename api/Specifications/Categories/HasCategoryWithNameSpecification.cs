using api.Models;
using Ardalis.Specification;

namespace api.Specifications.Categories
{
    public class HasCategoryWithNameSpecification : Specification<Category>
    {
        public HasCategoryWithNameSpecification(string appUserId, string name)
        {
            Query.Where(c => c.AppUserId == appUserId && c.Name == name);
        }
    }
}
