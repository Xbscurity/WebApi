namespace api.Dtos.Interfaces
{
    /// <summary>
    /// Defines a contract for objects that are associated with a specific category.
    /// </summary>
    /// <remarks>
    /// Implement this interface to indicate that an object has a related category identifier.
    /// This can be useful for generic data access, filtering, or categorization logic.
    /// </remarks>
    public interface IHasCategoryId
    {
        /// <summary>
        /// Gets the identifier of the associated category.
        /// </summary>
        int CategoryId { get; }
    }
}
