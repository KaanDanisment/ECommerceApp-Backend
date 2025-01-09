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

        [HttpGet("getall")]
        public async Task<IActionResult> GetlAll()
        {
            var result = await _productManager.GetAllAsync().ConfigureAwait(false);
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
            var result = await _productManager.GetByIdAsync(id).ConfigureAwait(false);
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
        public async Task<IActionResult> Add([FromForm] ProductCreateDto productCreateDto)
        {
            var result = await _productManager.AddAsync(productCreateDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<ProductCreateDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message =  result.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
            }
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(ProductUpdateDto productUpdateDto)
        {
            var result = await _productManager.UpdateAsync(productUpdateDto).ConfigureAwait(false);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productManager.DeleteAsync(id).ConfigureAwait(false);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbycategoryid/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            var result = await _productManager.GetProductsByCategoryId(categoryId).ConfigureAwait(false);
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
            var result = await _productManager.GetProductsBySubcategoryId(subcategoryId).ConfigureAwait(false);
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
            var result = await _productManager.GetLatestProductsAsync().ConfigureAwait(false);
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
