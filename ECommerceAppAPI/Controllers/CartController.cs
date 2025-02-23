using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly ICartService _cartManager;
        private readonly UserManager<User> _userManager;

        public CartController(ICartService cartManager, UserManager<User> userManager)
        {
            _cartManager = cartManager;
            _userManager = userManager;
        }

        [HttpPost("addToCart")]
        public async Task<IActionResult> AddToCart(CartItemCreateDto createCartItemDto)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);

            var result = await _cartManager.AddCartItemToCart(user, createCartItemDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(errorResult.Message);
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                }
            }
            return Ok();
        }

        [HttpDelete("deletCart")]
        public async Task<IActionResult> DeleteCart()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);

            var result = await _cartManager.DeleteCartAsync(user).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return BadRequest(errorDataResult.Message);
                    }
                }
            }
            return Ok();
        }

        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            var result = await _cartManager.GetCartAsync(user).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<CartDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return BadRequest(errorDataResult.Message);
                    }
                    return Ok(new { Data = result.Data, Message = result.Message });
                }
            }
            return Ok(result.Data);
        }

        [HttpDelete("removeCartItem/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            var result = await _cartManager.RemoveCartItemFromCart(user, cartItemId).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return BadRequest(errorDataResult.Message);
                    }
                }
            }
            return Ok();
        }
    }
}
