using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CartManager : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisService _redisManager;
        private readonly UserManager<User> _userManager;

        public CartManager(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IRedisService redisManager, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _redisManager = redisManager;
            _userManager = userManager;
        }

        private string CreateGuestSessionId()
        {
            var guestSessionId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("GuestSessionId", guestSessionId, cookieOptions);
            return guestSessionId;
        }

        private async Task<IDataResult<Cart>> CreateCartAsync(string userId)
        {
            try
            {
                Cart cart = new Cart
                {
                    UserId = userId,
                    TotalPrice = 0,
                    LastUpdatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                User? user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    var db = _redisManager.GetDb(0);
                    var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
                    await db.StringSetAsync(userId, cartJson, TimeSpan.FromHours(1));
                }
                else
                {
                    await _unitOfWork.Carts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }
                return new SuccessDataResult<Cart>(cart);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<Cart>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
        public async Task<Core.Utilities.Results.Abstract.IResult> AddCartItemToCart(User user, CartItemCreateDto cartItemCreateDto)
        {
            Product product = await _unitOfWork.Products.GetAsync(p => p.Id == cartItemCreateDto.ProductId, include: p => p.Include(p => p.Images));
            if (product == null)
            {
                return new ErrorResult("Ürün bulunamadı", "NotFound");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                Cart cart;
                if (user == null)
                {
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    var db = _redisManager.GetDb(0);
                    var guestSessionId = _httpContextAccessor.HttpContext.Request.Cookies["GuestSessionId"];
                    if (guestSessionId == null)
                    {
                        guestSessionId = CreateGuestSessionId();

                        var result = await this.CreateCartAsync(guestSessionId);
                        if (!result.Success)
                        {
                            return new ErrorResult(result.Message, "SystemError");
                        }
                        cart = result.Data;
                    }
                    else
                    {
                        var cartJson = await db.StringGetAsync(guestSessionId);
                        if (!cartJson.HasValue)
                        {
                            var result = await this.CreateCartAsync(guestSessionId);
                            if (!result.Success)
                            {
                                return new ErrorResult(result.Message, "SystemError");
                            }
                            cart = result.Data;
                        }
                        
                        cart = System.Text.Json.JsonSerializer.Deserialize<Cart>(cartJson, options);
                    }

                    if (cart.CartItems != null && cart.CartItems.Count != 0)
                    {
                        CartItem existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItemCreateDto.ProductId);
                        if (existingCartItem != null)
                        {
                            existingCartItem.Quantity += cartItemCreateDto.Quantity;
                            existingCartItem.UnitPrice = existingCartItem.Quantity * product.Price;
                            existingCartItem.LastUpdatedAt = DateTime.Now;
                            cart.TotalPrice += product.Price;

                            var updatedCartJson = System.Text.Json.JsonSerializer.Serialize(cart,options);
                            await db.StringSetAsync(guestSessionId, updatedCartJson, TimeSpan.FromHours(1));
                            return new SuccessResult("Ürün sepete eklendi");
                        }
                    }

                    CartItem cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = cartItemCreateDto.ProductId,
                        Quantity = cartItemCreateDto.Quantity,
                        UnitPrice = cartItemCreateDto.Quantity * product.Price,
                        LastUpdatedAt = DateTime.Now,
                        Product = product
                    };
                    cart.CartItems.Add(cartItem);
                    cart.TotalPrice += cartItem.UnitPrice;

                    var newCartJson = System.Text.Json.JsonSerializer.Serialize(cart, options);
                    await db.StringSetAsync(guestSessionId, newCartJson, TimeSpan.FromHours(1));
                    await transaction.CommitAsync();
                    return new SuccessResult("Ürün sepete eklendi");
                }
                else
                {
                    cart = await _unitOfWork.Carts.GetAsync(c => c.UserId == user.Id, include: c => c.Include(c => c.CartItems));
                    if (cart == null)
                    {
                        var result = await this.CreateCartAsync(user.Id);
                        if (!result.Success)
                        {
                            return new ErrorResult(result.Message, "SystemError");
                        }
                        cart = result.Data;
                    }
                    if (cart.CartItems != null && cart.CartItems.Count != 0)
                    {
                        CartItem existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItemCreateDto.ProductId);
                        if (existingCartItem != null)
                        {
                            existingCartItem.Quantity += cartItemCreateDto.Quantity;
                            existingCartItem.UnitPrice = existingCartItem.Quantity * product.Price;
                            existingCartItem.LastUpdatedAt = DateTime.Now;
                            cart.TotalPrice += existingCartItem.UnitPrice;

                            await _unitOfWork.CartItems.UpdateAsync(existingCartItem);
                            await _unitOfWork.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return new SuccessResult("Ürün sepete eklendi");
                        }
                    }
                    CartItem cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = cartItemCreateDto.ProductId,
                        Quantity = cartItemCreateDto.Quantity,
                        UnitPrice = (cartItemCreateDto.Quantity) * product.Price,
                        LastUpdatedAt = DateTime.Now
                    };
                    cart.TotalPrice += cartItem.UnitPrice;

                    await _unitOfWork.CartItems.AddAsync(cartItem);
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new SuccessResult("Ürün sepete eklendi");
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public Task<Core.Utilities.Results.Abstract.IResult> ClearCartAsync(Cart cart)
        {
            throw new NotImplementedException();
        }

        public async Task<Core.Utilities.Results.Abstract.IResult> DeleteCartAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    var db = _redisManager.GetDb(0);
                    var guestSessionId = _httpContextAccessor.HttpContext.Request.Cookies["GuestSessionId"];
                    if (guestSessionId == null)
                    {
                        return new ErrorResult("Sepet bulunamadı", "NotFound");
                    }
                    await db.KeyDeleteAsync(_httpContextAccessor.HttpContext.Request.Cookies["GuestSessionId"]);
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("GuestSessionId");
                    return new SuccessResult("Sepet başarıyla silindi");
                }
                Cart cart = await _unitOfWork.Carts.GetAsync(c => c.UserId == user.Id);
                if (cart == null)
                {
                    return new ErrorResult("Sepet bulunamadı", "NotFound");
                }
                await _unitOfWork.Carts.DeleteAsync(cart);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessResult("Sepet başarıyla silindi");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<CartDto>> GetCartAsync(User user)
        {
            Cart cart;
            CartDto cartDto;

            try
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                };

                if (user == null)
                {
                    var guestSessionId = _httpContextAccessor.HttpContext.Request.Cookies["GuestSessionId"];
                    if (guestSessionId == null)
                    {
                        return new ErrorDataResult<CartDto>(new CartDto(), "Sepet Boş");
                    }
                    var db = _redisManager.GetDb(0);
                    var cartJson = await db.StringGetAsync(guestSessionId);
                    if (!cartJson.HasValue)
                    {
                        return new ErrorDataResult<CartDto>(new CartDto(), "Sepet Boş");
                    }
                    cart = System.Text.Json.JsonSerializer.Deserialize<Cart>(cartJson, options);
                    cartDto = new CartDto
                    {
                        Id = cart.Id,
                        UserId = cart.UserId,
                        TotalPrice = cart.TotalPrice,
                        CartItems = cart.CartItems.Select(ci => new CartItemDto
                        {
                            Id = ci.Id,
                            Quantity = ci.Quantity,
                            ProductId = ci.ProductId,
                            CartId = cart.Id,
                            UnitPrice = ci.UnitPrice,
                            ProductDto = new ProductDto
                            {
                                Id = ci.ProductId,
                                Name = ci.Product.Name,
                                Color = ci.Product.Color,
                                Size = ci.Product.Size,
                                Description = ci.Product.Description,
                                Price = ci.Product.Price,
                                Stock = ci.Product.Stock,
                                CategoryId = ci.Product.CategoryId,
                                SubcategoryId = ci.Product.SubcategoryId,
                                CreatedAt = ci.Product.CreatedAt,
                                ImageUrls = ci.Product.Images.Select(i => i.FileUrl).ToList()
                            },
                            CreatedAt = ci.CreatedAt,
                            LastUpdatedAt = ci.LastUpdatedAt
                        }).ToList(),
                        CreatedAt = cart.CreatedAt,
                        LastUpdatedAt = cart.LastUpdatedAt
                    };
                    return new SuccessDataResult<CartDto>(cartDto);

                }
                cart = await _unitOfWork.Carts.GetAsync(c => c.UserId == user.Id, include: c => c.Include(c => c.CartItems));
                if (cart == null)
                {
                    return new ErrorDataResult<CartDto>(new CartDto(), "Sepet Boş");
                }

                cartDto = new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    TotalPrice = cart.TotalPrice,
                    CartItems = cart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        ProductId = ci.ProductId,
                        CartId = cart.Id,
                        UnitPrice = ci.UnitPrice,
                        CreatedAt = ci.CreatedAt,
                        LastUpdatedAt = ci.LastUpdatedAt
                    }).ToList(),
                    CreatedAt = cart.CreatedAt,
                    LastUpdatedAt = cart.LastUpdatedAt
                };
                return new SuccessDataResult<CartDto>(cartDto);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<CartDto>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<Core.Utilities.Results.Abstract.IResult> RemoveCartItemFromCart(User user, string cartItemId)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                };

                Cart cart;
                CartItem cartItemAvailable;
                if (user == null)
                {
                    var db = _redisManager.GetDb(0);
                    var guestSessionId = _httpContextAccessor.HttpContext.Request.Cookies["GuestSessionId"];
                    if (guestSessionId == null)
                    {
                        return new ErrorResult("Sepet bulunamadı", "NotFound");
                    }
                    var cartJson = await db.StringGetAsync(guestSessionId);
                    cart = System.Text.Json.JsonSerializer.Deserialize<Cart>(cartJson,options);
                    cartItemAvailable = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                    if (cartItemAvailable == null)
                    {
                        return new ErrorResult("Sepet ürünü bulunamadı", "NotFound");
                    }
                    cart.CartItems.Remove(cartItemAvailable);
                    if(cart.CartItems.Count == 0)
                    {
                        await db.KeyDeleteAsync(guestSessionId);
                        _httpContextAccessor.HttpContext.Response.Cookies.Delete("GuestSessionId");
                        return new SuccessResult("Ürünü sepetten çıkarıldı");
                    }
                    cart.TotalPrice -= cartItemAvailable.UnitPrice;
                    var updatedCartJson = System.Text.Json.JsonSerializer.Serialize(cart);
                    await db.StringSetAsync(guestSessionId, updatedCartJson);
                    return new SuccessResult("Ürünü sepetten çıkarıldı");
                }
                cart = await _unitOfWork.Carts.GetAsync(c => c.UserId == user.Id, include: c => c.Include(c=> c.CartItems));
                if (cart == null)
                {
                    return new ErrorResult("Sepet bulunamadı", "NotFound");
                }
                cartItemAvailable = await _unitOfWork.CartItems.GetAsync(ci => ci.Id == cartItemId);
                if (cartItemAvailable == null)
                {
                    return new ErrorResult("Sepet ürünü bulunamadı", "NotFound");
                }
                cart.TotalPrice -= cartItemAvailable.UnitPrice;
                await _unitOfWork.CartItems.DeleteAsync(cartItemAvailable);
                await _unitOfWork.SaveChangesAsync();
                if(cart.CartItems.Count == 0)
                {
                    await _unitOfWork.Carts.DeleteAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }
                return new SuccessResult("Ürünü sepetten çıkarıldı");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
    }
}
