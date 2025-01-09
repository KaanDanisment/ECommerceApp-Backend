using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using Entities.Validators;
using Microsoft.Data.SqlClient;
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

        public ProductManager(IUnitOfWork unitOfWork, IImageService imageManager, IAwsS3Service awsS3Manager)
        {
            _unitOfWork = unitOfWork;
            _awsS3Manager = awsS3Manager;
        }

        public async Task<IDataResult<ProductCreateDto>> AddAsync(ProductCreateDto productCreateDto)
        {
            var validator = new ProductCreateDtoValidator();
            var validationResults = await validator.ValidateAsync(productCreateDto).ConfigureAwait(false);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                if (errorMessage != null)
                {
                    return new ErrorDataResult<ProductCreateDto>(errorMessage,"BadRequest");
                }
            }

            if(productCreateDto.Images == null || !productCreateDto.Images.Any())
            {
                return new ErrorDataResult<ProductCreateDto>("En az bir ürün resmi ekleyin.", "BadRequest");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

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
                await _unitOfWork.Products.AddAsync(product).ConfigureAwait(false);

                List<Image> images = new List<Image>();
                foreach (var image in productCreateDto.Images)
                {
                    var fileUrl = await _awsS3Manager.UploadFileAsync(image).ConfigureAwait(false);
                    if (!fileUrl.Success)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        return new ErrorDataResult<ProductCreateDto>(fileUrl.Message, "SystemError");
                    }
                    images.Add(new Image()
                    {
                        FileUrl = fileUrl.Data,
                        Product = product
                    });
                }

                await _unitOfWork.Images.AddRangeAsync(images).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);
                return new SuccessDataResult<ProductCreateDto>(productCreateDto);
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                if (dbEx.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627)
                    {
                        return new ErrorDataResult<ProductCreateDto>(productCreateDto, "Aynı isme sahip bir ürün zaten mevcut.", "BadRequest");
                    }
                }
                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorDataResult<ProductCreateDto>(productCreateDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<ProductCreateDto>(productCreateDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            Product product = await _unitOfWork.Products.GetAsync(p => p.Id == id).ConfigureAwait(false);
            if (product != null)
            {
                Order order = await _unitOfWork.Orders.GetAsync(o => o.OrderProducts.Any(op => op.ProductId == product.Id)).ConfigureAwait(false);
                if (order != null && order.Status != "delivered")
                {
                    return new ErrorResult("Ürünü silebilmek için tüm siparişlerin tamamlanmasını bekleyin.");
                }
                await _unitOfWork.Products.DeleteAsync(product).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                return new SuccessResult("Ürün başarıyla silindi!");
            }
            return new ErrorResult("Ürün bulunamadı!");
        }

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetAllAsync()
        {
            try
            {
                IEnumerable<Product> products = await _unitOfWork.Products.GetAllAsync().ConfigureAwait(false);
                if (products == null || !products.Any())
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>(Array.Empty<ProductDto>(), "Ürün listesi boş.");
                }

                List<ProductDto> productDtos = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    SubcategoryId = p.SubcategoryId,
                    CreatedAt = p.CreatedAt
                }).ToList();

                IEnumerable<Image> allImages = await _unitOfWork.Images.GetAllAsync().ConfigureAwait(false);
                foreach (ProductDto productDto in productDtos)
                {
                    List<string> imageUrls = allImages.Where(img => img.ProductId == productDto.Id).Select(img => img.FileUrl).ToList();
                    productDto.ImageUrls = imageUrls;
                }
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

        public async Task<IDataResult<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                Product product = await _unitOfWork.Products.GetAsync(p => p.Id == id).ConfigureAwait(false);
                if (product != null)
                {
                    ProductDto productDto = new ProductDto()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Stock = product.Stock,
                        CategoryId = product.CategoryId,
                        SubcategoryId = product.SubcategoryId
                    };

                    IEnumerable<Image> images = await _unitOfWork.Images.GetImagesByProductIdAsync(productDto.Id).ConfigureAwait(false);
                    List<string> imageUrls = images.Select(image => image.FileUrl).ToList();
                    productDto.ImageUrls = imageUrls;

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

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetLatestProductsAsync()
        {
            try
            {
                IEnumerable<Product> products = await _unitOfWork.Products.GetLatestProducts().ConfigureAwait(false);
                if (products != null || products.Any())
                {
                    IEnumerable<ProductDto> producDtos = products.Select(p => new ProductDto()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        CategoryId = p.CategoryId,
                        SubcategoryId = p.SubcategoryId,
                        CreatedAt = p.CreatedAt
                    });
                    return new SuccessDataResult<IEnumerable<ProductDto>>(producDtos);
                }
                return new SuccessDataResult<IEnumerable<ProductDto>>(Array.Empty<ProductDto>(), "Ürün listesi boş.");
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

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId).ConfigureAwait(false);
                if (category == null)
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Product> products = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId).ConfigureAwait(false);
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
                    CategoryId = categoryId,
                    SubcategoryId = p.SubcategoryId
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

        public async Task<IDataResult<IEnumerable<ProductDto>>> GetProductsBySubcategoryId(int subcategoryId)
        {
            try
            {
                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryId).ConfigureAwait(false);
                if (subcategory == null)
                {
                    return new ErrorDataResult<IEnumerable<ProductDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Product> products = await _unitOfWork.Products.GetProductsBySubcategoryIdAsync(subcategoryId).ConfigureAwait(false);
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
                    CategoryId = p.CategoryId,
                    SubcategoryId = p.SubcategoryId
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

        public async Task<IResult> UpdateAsync(ProductUpdateDto productUpdateDto)
        {
            Product product = await _unitOfWork.Products.GetAsync(p => p.Id == productUpdateDto.Id).ConfigureAwait(false);
            if (product != null)
            {
                product.Name = productUpdateDto.Name;
                product.Color = productUpdateDto.Color;
                product.Size = productUpdateDto.Size;
                product.Description = productUpdateDto.Description;
                product.Price = productUpdateDto.Price;
                product.Stock = productUpdateDto.Stock;
                product.CategoryId = productUpdateDto.CategoryId;
                product.SubcategoryId = productUpdateDto.SubcategoryId;
                await _unitOfWork.Products.UpdateAsync(product).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                return new SuccessDataResult<ProductUpdateDto>("Product updated successfuly");
            }
            return new ErrorDataResult<ProductUpdateDto>(productUpdateDto, "Product could not be updated");
        }
    }
}
