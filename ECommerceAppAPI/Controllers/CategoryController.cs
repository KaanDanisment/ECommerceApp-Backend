using Business.Abstract;
using Core.Utilities.Results.Concrete;
using Entities;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryManager;

        public CategoryController(ICategoryService categoryManager)
        {
            _categoryManager = categoryManager;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryManager.GetAllAsync().ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<CategoryDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
                return Ok(result.Data);
            }
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryManager.GetByIdAsync(id).ConfigureAwait(false);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryCreateDto categoryCreateDto)
        {
            var result = await _categoryManager.AddAsync(categoryCreateDto).ConfigureAwait(false);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = ((CategoryCreateDto)result.Data).Id }, result);
            }
            return BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(CategoryUpdateDto categoryUpdateDto)
        {
            var result = await _categoryManager.UpdateAsync(categoryUpdateDto).ConfigureAwait(false);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryManager.DeleteAsync(id).ConfigureAwait(false);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
