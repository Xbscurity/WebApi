using api.Models;
using Ardalis.Specification;

namespace api.Specifications.Categories
{
    /// <summary>
    /// Specification for checking whether a category with the specified name
    /// already exists for a given user.
    /// </summary>
    public class HasCategoryWithNameSpecification : Specification<Category>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HasCategoryWithNameSpecification"/> class.
        /// </summary>
        /// <param name="appUserId">The identifier of the user who owns the category.</param>
        /// <param name="name">The category name to check for existence.</param>
        /// <param name="excludeId">
        /// An optional category identifier to exclude from the check.
        /// Used when updating a category, to avoid matching the category against itself.
        /// </param>
        public HasCategoryWithNameSpecification(string appUserId, string name, Guid? excludeId = null)
        {
            Query.Where(c => c.AppUserId == appUserId && c.Name == name);

            if (excludeId is not null)
            {
                Query.Where(c => c.Id != excludeId);
            }
        }
    }
}
