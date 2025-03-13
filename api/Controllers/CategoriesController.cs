﻿using System.ComponentModel;
using api.Converters;
using api.Data;
using api.Dtos;
using api.Dtos.Category;
using api.Filters;
using api.Helpers;
using api.Models;
using api.Repositories;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{

    
    [ServiceFilter(typeof(ExecutionTimeFilter))]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _categoriesRepository;
        public CategoriesController(ICategoriesRepository categoryRepository)
        {
            _categoriesRepository = categoryRepository;
        }
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A list of all categoeries</returns>
        [HttpGet]
        public async Task<ApiResponse<List<Category>>> GetAll()
        {
            var categories = await _categoriesRepository.GetAllAsync();

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
            var category = await _categoriesRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            return ApiResponse.Success(category);
        }
        /// <summary>
        /// CreateAsync a new category.
        /// </summary>
        /// <param name="categoryDto">The data for the new category.</param>
        /// <returns>The created category with its details</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPost]
        public async Task<ApiResponse<Category>> Create([FromBody] CategoryDto? categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto?.Name
            };
            await _categoriesRepository.CreateAsync(category);
            return ApiResponse.Success(category);
        }
        /// <summary>
        /// UpdateAsync an existing category.
        /// </summary>
        /// <param name="id">The Id of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPut("{id:int}")]
        public async Task<ApiResponse<Category>> Update([FromRoute] int id, [FromBody] CategoryDto categoryDto)
        {
            var category = await _categoriesRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            category.Name = categoryDto.Name;
            var updatedCategory = await _categoriesRepository.UpdateAsync(category);
            return ApiResponse.Success(updatedCategory);
        }
        /// <summary>
        /// DeleteAsync an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpDelete("{id:int}")]

        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            var success = await _categoriesRepository.DeleteAsync(id);
            if (!success)
            {
                return ApiResponse.NotFound<bool>("Category not found");
            }
            return ApiResponse.Success(true);
        }
        [HttpGet("convert")]
        public ApiResponse<TimeZoneRequest> GetTimeZoneInfo([FromQuery]TimeZoneRequest request)
        {
            if (request.TimeZone == null)
            {
                return ApiResponse.NotFound<TimeZoneRequest>("Invalid or missing timezone.");
            }

            return ApiResponse.Success(request);
        }
    }
}
