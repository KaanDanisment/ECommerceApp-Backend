using Business.Abstract;
using Core.Utilities.Results.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly IProductService _productManager;

        public ProductController(IProductService productManager)
        {
            _productManager = productManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetlAll()
        {
            var result = await _productManager.GetAllAsync();
            if (!result.Success)
            {
                if (result is ErrorDataResult<ProductDto> errorDataResult)
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
            var result = await _productManager.GetByIdAsync(id);
            if (!result.Success)
            {
                if (result is ErrorDataResult<ProductDto> errorDataResult)
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
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductCreateDto productCreateDto)
        {
            var result = await _productManager.AddAsync(productCreateDto);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = ((ProductCreateDto)result.Data).Id }, result);
            }
            return BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ProductUpdateDto productUpdateDto)
        {
            var result = await _productManager.UpdateAsync(productUpdateDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productManager.DeleteAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbycategoryid/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            var result = await _productManager.GetProductsByCategoryId(categoryId);
            if (!result.Success)
            {
                if (result is ErrorDataResult<IEnumerable<ProductDto>> errorDataResult)
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

        [HttpGet("getbysubcategoryid/{subcategoryId}")]
        public async Task<IActionResult> GetProductsBySubcategoryId(int subcategoryId)
        {
            var result = await _productManager.GetProductsBySubcategoryId(subcategoryId);
            if (!result.Success)
            {
                if (result is ErrorDataResult<IEnumerable<ProductDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { result.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    return Ok(result.Data);
                }
            }
            return Ok(result.Data);
        }

        [HttpGet("getlatestproducts")]
        public async Task<IActionResult> GetLatestProducts()
        {
            var result = await _productManager.GetLatestProductsAsync();
            if (!result.Success)
            {
                if (result is ErrorDataResult<ProductDto> errorDataResult)
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
    }
}
