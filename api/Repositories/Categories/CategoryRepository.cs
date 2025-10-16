using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Categories
{
    /// <summary>
    /// Provides Entity Framework Core–based implementation of <see cref="ICategoryRepository"/>.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ApplicationDbContext"/> for database operations on <see cref="Category"/> entities.
    /// </remarks>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context used for category data access.</param>
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        /// <inheritdoc />
        public IQueryable<Category> GetQueryable()
        {
            return _context.Categories.AsNoTracking();
        }

        /// <inheritdoc />
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task CreateAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task CreateRangeAsync(IEnumerable<Category> categories)
        {
            await _context.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
    }
}
