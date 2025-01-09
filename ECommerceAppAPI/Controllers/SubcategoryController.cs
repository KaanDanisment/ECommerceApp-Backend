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
            var result = await _subcategoryManager.GetAllSubcategoriesAsync().ConfigureAwait(false);
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
            var result = await _subcategoryManager.GetSubcategoryByIdAsync(id).ConfigureAwait(false);
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

        [HttpGet("getbycategoryid/{id}")]
        public async Task<IActionResult> GetSubcategoriesByCategoryId(int id)
        {
            var result = await _subcategoryManager.GetAllSubcategoriesByCategoryIdAsync(id).ConfigureAwait(false);
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
        public async Task<IActionResult> Add([FromForm] SubcategoryCreateDto subcategoryCreateDto)
        {
            var result = await _subcategoryManager.AddAsync(subcategoryCreateDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = errorResult.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new {Message = errorResult.Message});
                    }
                }
            }
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] SubcategoryCreateDto subcategoryUpdateDto)
        {
            var result = await _subcategoryManager.UpdateSubcategoryAsync(subcategoryUpdateDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = result.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = errorResult.Message });
                    }
                }
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _subcategoryManager.DeleteSubcategoryAsync(id).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = result.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = errorResult.Message });
                    }
                }
            }
            return Ok();
        }
    }
}
