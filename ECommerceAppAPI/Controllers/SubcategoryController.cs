using Business.Abstract;
using Business.Concrete;
using Core.Utilities.Results.Concrete;
using Entities;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController : Controller
    {
        private readonly ISubcategoryService _subcategoryManager;

        public SubcategoryController(ISubcategoryService subcategoryManager)
        {
            _subcategoryManager = subcategoryManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubcategories()
        {
            var result = await _subcategoryManager.GetAllSubcategoriesAsync();
            if (!result.Success)
            {
                if (result is ErrorDataResult<SubcategoryDto> errorDataResult)
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
        public async Task<IActionResult> GetSubcategorieById(int id)
        {
            var result = await _subcategoryManager.GetSubcategoryByIdAsync(id);
            if (!result.Success)
            {
                if (result is ErrorDataResult<SubcategoryDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { result.Message });
                    }
                }
            }
            return Ok(result.Data);
        }

        [HttpGet("getallbycategoryid/{id}")]
        public async Task<IActionResult> GetAllSubcategoriesByCategoryId(int id)
        {
            var result = await _subcategoryManager.GetAllSubcategoriesByCategoryIdAsync(id);
            if (!result.Success)
            {
                if (result is ErrorDataResult<IEnumerable<SubcategoryDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { result.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
                return Ok(result.Data);
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add(SubcategoryCreateDto subcategoryCreateDto)
        {
            var result = await _subcategoryManager.AddSubcategoryAsync(subcategoryCreateDto);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetSubcategorieById), new { id = ((SubcategoryCreateDto)result.Data).Id }, result);
            }
            return BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(SubcategoryUpdateDto subcategoryUpdateDto)
        {
            var result = await _subcategoryManager.UpdateSubcategoryAsync(subcategoryUpdateDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _subcategoryManager.DeleteSubcategoryAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}
