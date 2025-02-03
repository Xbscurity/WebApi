using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Category;
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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CategoryDto categoryDto)
        {
             Category category = new Category
            {
                Name = categoryDto.Name
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
          return CreatedAtAction(nameof(GetById), new { id = category.Id}, category);  
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody]CategoryDto categoryDto)
        {
             var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            category.Name = categoryDto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
             var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
