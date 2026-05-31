using api.Data;
using Ardalis.Specification.EntityFrameworkCore;

namespace api.Interfaces
{
    /// <summary>
    /// Provides a generic repository implementation.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type managed by the repository.
    /// </typeparam>
    /// <remarks>
    /// This repository extends <see cref="RepositoryBase{T}"/>
    /// and provides common CRUD and specification-based query operations.
    /// </remarks>
    public class EfRepository<T> : RepositoryBase<T>, IRepository<T>
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfRepository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">
        /// The database context used for entity persistence.
        /// </param>
        public EfRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
