using api.Data;
using api.Dtos.Category;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A list of all categoeries</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<Category>>))]
        [HttpGet]
        public async Task<ApiResponse<List<Category>>> GetAll()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return ApiResponse.Success(categories);
        }
        /// <summary>
        /// Get a specific category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to get.</param>
        /// <returns>The category with the specified ID.</returns>
         [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpGet("{id:int}")]
        public async Task<ApiResponse<Category>> GetById([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            return ApiResponse.Success(category);
        }
        /// <summary>
        /// Create a new category.
        /// </summary>
        /// <param name="categoryDto">The data for the new category.</param>
        /// <returns>The created category with its details</returns>
         [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPost]
        public async Task<ApiResponse<Category>> Create([FromBody] CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return ApiResponse.NoContent<Category>();
        }
        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="id">The Id of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPut("{id:int}")]
        public async Task<ApiResponse<Category>> Update([FromRoute] int id, [FromBody] CategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            category.Name = categoryDto.Name;
            await _context.SaveChangesAsync();
            return ApiResponse.NoContent<Category>();
        }
        /// <summary>
        /// Delete an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpDelete("{id:int}")]

        public async Task<ActionResult<ApiResponse<Category>>> Delete([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return ApiResponse.NoContent<Category>();
        }
       
    }
}
