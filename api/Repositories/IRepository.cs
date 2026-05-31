using Ardalis.Specification;

namespace api.Interfaces
{
    /// <summary>
    /// Defines a generic repository abstraction for entity persistence operations.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type managed by the repository.
    /// </typeparam>
    /// <remarks>
    /// Extends <see cref="IRepositoryBase{T}"/> with application-specific
    /// repository contracts.
    /// </remarks>
    public interface IRepository<T> : IRepositoryBase<T>
        where T : class
    {
    }
}
