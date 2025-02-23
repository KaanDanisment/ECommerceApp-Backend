using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICartService
    {
        Task<IDataResult<CartDto>> GetCartAsync(User user);
        Task<IResult> DeleteCartAsync(User user);
        Task<IResult> AddCartItemToCart(User user, CartItemCreateDto createCartItemDto);
        Task<IResult> RemoveCartItemFromCart(User user, string cartItemId);
        Task<IResult> ClearCartAsync(Cart cart);
    }
}
