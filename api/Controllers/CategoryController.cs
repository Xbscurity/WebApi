using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Category;
using api.Helpers;
using api.Helpers.Report;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<FinancialTransaction>))]
        [HttpGet]
        public async Task<ActionResult<Response<List<Category>>>> GetAll()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }
        /// <summary>
        /// Get a specific category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to get.</param>
        /// <returns>The category with the specified ID.</returns>
         [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<Category>))]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Response<Category>>> GetById([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }
        /// <summary>
        /// Create a new category.
        /// </summary>
        /// <param name="categoryDto">The data for the new category.</param>
        /// <returns>The created category with its details</returns>
         [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Response<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<Category>))]
        [HttpPost]
        public async Task<ActionResult<Response<Category>>> Create([FromBody] CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, Response<Category>.SuccessResponse(category));
        }
        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="id">The Id of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<Category>))]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Response<Category>>> Update([FromRoute] int id, [FromBody] CategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(Response<Category>.NotFoundResponse("Category not found"));
            }
            category.Name = categoryDto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Delete an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response<Category>))]
        [HttpDelete("{id:int}")]

        public async Task<ActionResult<Response<Category>>> Delete([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(Response<Category>.NotFoundResponse("Category not found"));
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
       
    }
}
