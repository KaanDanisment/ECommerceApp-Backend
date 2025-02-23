using Business.Abstract;
using Core.Utilities.GroupedProductsResult;
using Core.Utilities.Pagination;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using Entities.Validators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAwsS3Service _awsS3Manager;
        private readonly IImageService _imageManager;

        public ProductManager(IUnitOfWork unitOfWork, IImageService imageManager, IAwsS3Service awsS3Manager)
        {
            _unitOfWork = unitOfWork;
            _awsS3Manager = awsS3Manager;
            _imageManager = imageManager;
        }

        public async Task<IDataResult<ProductCreateDto>> AddAsync(ProductCreateDto productCreateDto)
        {
            var validator = new ProductCreateDtoValidator();
            var validationResults = await validator.ValidateAsync(productCreateDto);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                if (errorMessage != null)
                {
                    return new ErrorDataResult<ProductCreateDto>(errorMessage, "BadRequest");
                }
            }

            if (productCreateDto.Images == null || !productCreateDto.Images.Any())
            {
                return new ErrorDataResult<ProductCreateDto>("En az bir ürün resmi ekleyin.", "BadRequest");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                Product product = new Product()
                {
                    Name = productCreateDto.Name,
                    Color = productCreateDto.Color,
                    Size = productCreateDto.Size,
                    Description = productCreateDto.Description,
                    Price = productCreateDto.Price,
                    Stock = productCreateDto.Stock,
                    CategoryId = productCreateDto.CategoryId,
                    SubcategoryId = productCreateDto.SubcategoryId,
                };
                await _unitOfWork.Products.AddAsync(product);

                var result = await _imageManager.AddAsync(productCreateDto.Images, product.Id);
                if (!result.Success)
                {
                    await transaction.RollbackAsync();
                    return new ErrorDataResult<ProductCreateDto>(result.Message, "SystemError");
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SuccessDataResult<ProductCreateDto>(productCreateDto);
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorDataResult<ProductCreateDto>(productCreateDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<ProductCreateDto>(productCreateDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
        public async Task<IResult> UpdateAsync(ProductUpdateDto productUpdateDto)
        {
            Product product = await _unitOfWork.Products.GetAsync(p => p.Id == productUpdateDto.Id);
            if (product == null)
            {
                return new ErrorResult("Ürün bulunamadı!", "NotFound");
            }

            var validator = new ProductUpdateDtoValidator();
            var validationResults = await validator.ValidateAsync(productUpdateDto);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                if (errorMessage != null)
                {
                    return new ErrorResult(errorMessage, "BadRequest");
                }
            }

            if (productUpdateDto.Images == null || !productUpdateDto.Images.Any())
            {
                return new ErrorResult("En az bir ürün resmi ekleyin.", "BadRequest");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                product.Name = productUpdateDto.Name;
                product.Color = productUpdateDto.Color;
                product.Size = productUpdateDto.Size;
                product.Description = productUpdateDto.Description;
                product.Price = productUpdateDto.Price;
                product.Stock = productUpdateDto.Stock;

                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == productUpdateDto.CategoryId);
                if (category == null)
                {
                    return new ErrorResult("Geçersiz kategori", "BadRequest");
                }
                product.CategoryId = productUpdateDto.CategoryId;

                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == productUpdateDto.SubcategoryId);
                if (subcategory == null)
                {
                    return new ErrorResult("Geçersiz alt kategori", "BadRequest");
                }
                product.SubcategoryId = productUpdateDto.SubcategoryId;

                if (productUpdateDto.Images[0].Length > 0)
                {
                    var existingImages = await _unitOfWork.Images.GetImagesByProductIdAsync(product.Id);

                    // Ürünün mevcut resimlerini aws bucketten sil
                    foreach (var image in existingImages)
                    {
                        var key = image.FileUrl.Substring(image.FileUrl.LastIndexOf('/') + 1);
                        var result = await _awsS3Manager.DeleteFileAsync(key);
                        if (!result.Success)
                        {
                            return new ErrorResult(result.Message, "SystemError");
                        }
                    }

                    // Eğer mevcut resim sayısı, güncellenecek resim sayısından fazla ise, fazla olan resimleri sil
                    if (existingImages.Count() > productUpdateDto.Images.Count)
                    {
                        var imagesToDelete = existingImages.Skip(productUpdateDto.Images.Count).ToList();
                        foreach (var image in imagesToDelete)
                        {
                            await _unitOfWork.Images.DeleteAsync(image);
                        }
                    }

                    int index = 0;
                    foreach (var image in productUpdateDto.Images)
                    {
                        var fileUrl = await _awsS3Manager.UploadFileAsync(image);
                        if (!fileUrl.Success)
                        {
                            await transaction.RollbackAsync();
                            return new ErrorResult(fileUrl.Message, "SystemError");
                        }

                        if (index < existingImages.Count())
                        {
                            var existingImage = existingImages.ElementAt(index);
                            existingImage.FileUrl = fileUrl.Data;
                            await _unitOfWork.Images.UpdateAsync(existingImage);
                        }
                        else
                        {
                            var newImage = new Image()
                            {
                                FileUrl = fileUrl.Data,
                                Product = product
                            };
                            await _unitOfWork.Images.AddAsync(newImage);
                        }
                        index++;
                    }
                }

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SuccessResult("Ürün başarıyla güncellendi!");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
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
        public async Task<IResult> DeleteAsync(int id)
        {
            try
            {
                Product product = await _unitOfWork.Products.GetAsync(p => p.Id == id, include: p => p.Include(p => p.Images));
                if (product != null)
                {
                    if (product.OrderProducts != null && product.OrderProducts.Any())
                    {
                        var orderIds = product.OrderProducts.Select(op => op.OrderId).ToList();
                        if (orderIds.Any())
                        {
                            List<Order> orders = (await _unitOfWork.Orders.GetAllAsync(o => orderIds.Contains(o.Id))).ToList();
                            if (orders != null && orders.Any(o => o.Status != "delivered"))
                            {
                                return new ErrorResult("Ürünü silebilmek için tüm siparişlerin tamamlanmasını bekleyin.", "BadRequest");
                            }
                        }
                    }

                    var sameNameProducts = await _unitOfWork.Products.GetAllAsync(p => p.Name == product.Name);
                    if (!sameNameProducts.Any())
                    {
                        foreach (var image in product.Images)
                        {
                            var key = image.FileUrl.Split("/").Last();
                            await _awsS3Manager.DeleteFileAsync(key);
                        }
                    }

                    await _unitOfWork.Products.DeleteAsync(product);
                    await _unitOfWork.SaveChangesAsync();
                    return new SuccessResult("Ürün başarıyla silindi!");
                }
                return new ErrorResult("Ürün bulunamadı!", "NotFound");
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

        private async Task<IDataResult<IEnumerable<Product>>> SortProducts(int? page, string sortBy)
        {
            try
            {
                IEnumerable<Product> products;

                int pageSize = 20;
                int pageNumber = page ?? 0;

                Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null;
                switch (sortBy)
                {
                    case "id_descending":
                        orderBy = q => q.OrderByDescending(p => p.Id);
                        break;
                    case "price_ascending":
                        orderBy = q => q.OrderBy(p => p.Price);
                        break;
                    case "price_descending":
                        orderBy = q => q.OrderByDescending(p => p.Price);
                        break;
                    case "stock_ascending":
                        orderBy = q => q.OrderBy(p => p.Stock);
                        break;
                    case "stock_descending":
                        orderBy = q => q.OrderByDescending(p => p.Stock);
                        break;
                    case "date_ascending":
                        orderBy = q => q.OrderBy(p => p.CreatedAt);
                        break;
                    case "date_descending":
                        orderBy = q => q.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                products = await _unitOfWork.Products.GetAllAsync(
                    include: p => p.Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory),
                    orderBy: orderBy
                ).ConfigureAwait(false);

                if (pageNumber != 0)
                {
                    products = products
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                return new SuccessDataResult<IEnumerable<Product>>(products);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<Product>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
        public async Task<IDataResult<PaginationResult<ProductDto>>> GetAllAsync(int? page, string? sortBy)
        {

            try
            {
                IEnumerable<Product> products;
                int totalItemCount = (await _unitOfWork.Products.GetAllAsync().ConfigureAwait(false)).Count();
                int pageSize = 20;
                int pageNumber = page ?? 0;

                if (sortBy == null)
                {
                    if (page > 0)
                    {
                        products = (await _unitOfWork.Products.GetAllAsync(
                            include: p => p.Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)).ConfigureAwait(false))
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
                    else
                    {
                        pageSize = totalItemCount;
                        products = await _unitOfWork.Products.GetAllAsync(
                            include: p => p.Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)).ConfigureAwait(false);
                    }
                }
                else
                {
                    if (page > 0)
                    {
                        var result = await SortProducts(page, sortBy);
                        if (result.Success)
                        {
                            products = result.Data;
                        }
                        else
                        {
                            return new ErrorDataResult<PaginationResult<ProductDto>>(
                                new PaginationResult<ProductDto>(Array.Empty<ProductDto>(), totalItemCount, pageNumber, pageSize), "SystemError");
                        }
                    }
                    else
                    {
                        var result = await SortProducts(page, sortBy);
                        if (result.Success)
                        {
                            products = result.Data;
                        }
                        else
                        {
                            return new ErrorDataResult<PaginationResult<ProductDto>>(
                                new PaginationResult<ProductDto>(Array.Empty<ProductDto>(), totalItemCount, pageNumber, pageSize), "SystemError");
                        }
                    }
                }


                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<PaginationResult<ProductDto>>(
                        new PaginationResult<ProductDto>(Array.Empty<ProductDto>(), totalItemCount, pageNumber, pageSize), "Ürün listesi boş.");
                }

                IEnumerable<ProductDto> productDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    SubcategoryName = p.Subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                }).ToList();

                var paginationResult = new PaginationResult<ProductDto>(productDtos, totalItemCount, pageNumber, pageSize);

                return new SuccessDataResult<PaginationResult<ProductDto>>(paginationResult);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<PaginationResult<ProductDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                Product product = await _unitOfWork.Products.GetAsync(filter: p => p.Id == id, include: p => p.Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)).ConfigureAwait(false);
                if (product != null)
                {
                    ProductDto productDto = new ProductDto()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Color = product.Color,
                        Size = product.Size,
                        Description = product.Description,
                        Price = product.Price,
                        Stock = product.Stock,
                        CategoryName = product.Category.Name,
                        CategoryId = product.CategoryId,
                        SubcategoryName = product.Subcategory.Name,
                        SubcategoryId = product.SubcategoryId,
                        CreatedAt = product.CreatedAt,
                        ImageUrls = product.Images.Select(img => img.FileUrl).ToList()
                    };

                    return new SuccessDataResult<ProductDto>(productDto);
                }
                return new ErrorDataResult<ProductDto>("Aradığınız ürün bulunamadı!", "NotFound");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<ProductDto>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetLatestProductsAsync()
        {
            try
            {
                IEnumerable<Product> products = await _unitOfWork.Products.GetLatestProducts().ConfigureAwait(false);
                if (products != null || products.Any())
                {
                    IEnumerable<ProductDto> productDtos = products.Select(p => new ProductDto()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        Color = p.Color,
                        Size = p.Size,
                        CategoryName = p.Category.Name,
                        CategoryId = p.CategoryId,
                        SubcategoryName = p.Subcategory.Name,
                        SubcategoryId = p.SubcategoryId,
                        CreatedAt = p.CreatedAt,
                        ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                    });

                    IEnumerable<GroupedProductsResult<ProductDto>> groupedProductsResult = productDtos
                   .GroupBy(p => new { p.Name, p.Color })
                   .Select(g => new GroupedProductsResult<ProductDto>(g))
                   .ToList();
                    return new SuccessDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(groupedProductsResult);
                }
                return new SuccessDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(Array.Empty<GroupedProductsResult<ProductDto>>(), "Ürün listesi boş.");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetProductsByCategoryId(int categoryId, string? sortBy)
        {
            try
            {
                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId).ConfigureAwait(false);
                if (category == null)
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Product> products;

                products = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId, sortBy).ConfigureAwait(false);

                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>(Array.Empty<ProductDto>(), "Aradığınız kategoride ürün bulunamadı!");
                }

                IEnumerable<ProductDto> productDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Color = p.Color,
                    Size = p.Size,
                    CategoryName = p.Category.Name,
                    CategoryId = categoryId,
                    SubcategoryName = p.Subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                });
                return new SuccessDataResult<IEnumerable<ProductDto>>(productDtos);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<ProductDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetProductsBySubcategoryId(int subcategoryId, string? sortBy)
        {
            try
            {
                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryId).ConfigureAwait(false);
                if (subcategory == null)
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Product> products;

                products = await _unitOfWork.Products.GetProductsBySubcategoryIdAsync(subcategoryId, sortBy).ConfigureAwait(false);

                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>(Array.Empty<ProductDto>(), "Aradığınız kategoride ürün bulunamadı!");
                }
                IEnumerable<ProductDto> producDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Color = p.Color,
                    Size = p.Size,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    SubcategoryName = subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                });
                return new SuccessDataResult<IEnumerable<ProductDto>>(producDtos);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<ProductDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetGroupedProductsByCategoryId(int categoryId, string? sortBy)
        {
            try
            {
                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId).ConfigureAwait(false);
                if (category == null)
                {
                    return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>("Aradığınız kategori bulunamadı!", "NotFound");
                }

                IEnumerable<Product> products = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId, sortBy);

                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(Array.Empty<GroupedProductsResult<ProductDto>>(), "Aradığınız kategoride ürün bulunamadı!");
                }

                IEnumerable<ProductDto> productDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    SubcategoryName = p.Subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                });

                IEnumerable<GroupedProductsResult<ProductDto>> groupedProductsResult = productDtos
                    .GroupBy(p => new { p.Name, p.Color })
                    .Select(g => new GroupedProductsResult<ProductDto>(g))
                    .ToList();
                return new SuccessDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(groupedProductsResult);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetGroupedProductsBySubcategoryId(int subcategoryId, string? sortBy)
        {
            try
            {
                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryId).ConfigureAwait(false);
                if (subcategory == null)
                {
                    return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>("Aradığınız alt kategori bulunamadı!", "NotFound");
                }

                IEnumerable<Product> products = await _unitOfWork.Products.GetProductsBySubcategoryIdAsync(subcategoryId, sortBy);
                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(Array.Empty<GroupedProductsResult<ProductDto>>(), "Aradığınız alt kategoride ürün bulunamadı!");
                }

                IEnumerable<ProductDto> productDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    SubcategoryName = p.Subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                });
                IEnumerable<GroupedProductsResult<ProductDto>> groupedProductsResult = productDtos
                        .GroupBy(p => new { p.Name, p.Color })
                        .Select(g => new GroupedProductsResult<ProductDto>(g))
                        .ToList();

                return new SuccessDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(groupedProductsResult);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<IDataResult<GroupedProductsResult<ProductDto>>> GetGroupedProductByProductId(int productId)
        {
            try
            {
                Product? product = await _unitOfWork.Products.GetAsync(p => p.Id == productId,
                    include: p => p.Include(p => p.Images)
                    .Include(p => p.Category)
                    .Include(p => p.Subcategory))
                    .ConfigureAwait(false);

                if (product == null)
                {
                    return new ErrorDataResult<GroupedProductsResult<ProductDto>>("Aradığınız ürün bulunamadı!", "NotFound");
                }

                IEnumerable<Product> products = await _unitOfWork.Products.GetProductsBySubcategoryIdAsync(product.SubcategoryId, null).ConfigureAwait(false);

                IEnumerable<Product> resultProducts = products.Where(p => p.Name == product.Name && p.Color == product.Color);

                IEnumerable<ProductDto> productDtos = resultProducts.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    SubcategoryName = p.Subcategory.Name,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(img => img.FileUrl).ToList()
                });

                GroupedProductsResult<ProductDto> groupedProductsResult = new GroupedProductsResult<ProductDto>(productDtos);
                return new SuccessDataResult<GroupedProductsResult<ProductDto>>(groupedProductsResult);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<GroupedProductsResult<ProductDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
    }
}