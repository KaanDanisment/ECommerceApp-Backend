using Business.Abstract;
using Core.Utilities.GroupedProductsResult;
using Core.Utilities.Pagination;
using Core.Utilities.Results.Abstract;
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
        public async Task<IActionResult> GetlAll([FromQuery] int? page, [FromQuery] string? sortBy)
        {
            var result = await _productManager.GetAllAsync(page, sortBy).ConfigureAwait(false);
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
                        return BadRequest(new { Message = result.Message });
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
        public async Task<IActionResult> Update([FromForm] ProductUpdateDto productUpdateDto)
        {
            var result = await _productManager.UpdateAsync(productUpdateDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = result.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = result.Message });
                    }
                }
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productManager.DeleteAsync(id).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = result.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = result.Message });
                    }
                }
            }
            return Ok();
        }

        [HttpGet("getbycategoryid/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId, [FromQuery] string? sortBy)
        {
            var result = await _productManager.GetProductsByCategoryId(categoryId, sortBy).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<IEnumerable<ProductDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = result.Message });
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
        public async Task<IActionResult> GetProductsBySubcategoryId(int subcategoryId, [FromQuery] string? sortBy)
        {
            var result = await _productManager.GetProductsBySubcategoryId(subcategoryId, sortBy).ConfigureAwait(false);
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

        [HttpGet("getGroupedProductsByCategoryId/{categoryId}")]
        public async Task<IActionResult> GetGroupedProductsByCategoryId(int categoryId, [FromQuery] string? sortBy)
        {
            var result = await _productManager.GetGroupedProductsByCategoryId(categoryId, sortBy).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<GroupedProductsResult<ProductDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    return Ok(result.Data);
                }
            }
            return Ok(result.Data);
        }

        [HttpGet("getGroupedProductsBySubcategoryId/{subcategoryId}")]
        public async Task<IActionResult> GetGroupedProductsBySubcategoryId(int subcategoryId, [FromQuery] string? sortBy)
        {
            var result = await _productManager.GetGroupedProductsBySubcategoryId(subcategoryId, sortBy).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<GroupedProductsResult<ProductDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    return Ok(result.Data);
                }
            }
            return Ok(result.Data);
        }

        [HttpGet("getGroupedProductByProductId/{productId}")]
        public async Task<IActionResult> GetGroupedProductByProductId(int productId)
        {
            var result = await _productManager.GetGroupedProductByProductId(productId).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<GroupedProductsResult<ProductDto>> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
            }
            return Ok(result.Data);
        }
    }
}
