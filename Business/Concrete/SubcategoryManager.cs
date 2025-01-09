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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class SubcategoryManager : ISubcategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAwsS3Service _awsS3Manager;

        public SubcategoryManager(IUnitOfWork unitOfWork, IAwsS3Service awsS3Manager)
        {
            _unitOfWork = unitOfWork;
            _awsS3Manager = awsS3Manager;
        }

        public async Task<IResult> AddAsync(SubcategoryCreateDto subcategoryCreateDto)
        {
            var validator = new SubcategoryCreateDtoValidator();
            var validationResults = await validator.ValidateAsync(subcategoryCreateDto).ConfigureAwait(false);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                if (errorMessage != null)
                {
                    return new ErrorResult(errorMessage, "BadRequest");
                }
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                Subcategory subcategory = new Subcategory()
                {
                    Name = subcategoryCreateDto.Name,
                    CategoryId = subcategoryCreateDto.CategoryId,
                };

                if (subcategoryCreateDto.Image != null)
                {
                    var fileUrl = await _awsS3Manager.UploadFileAsync(subcategoryCreateDto.Image);
                    if (!fileUrl.Success)
                    {
                        return new ErrorResult(fileUrl.Message, "SystemError");
                    }
                    subcategory.ImageUrl = fileUrl.Data;
                }

                await _unitOfWork.Subcategories.AddAsync(subcategory).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);
                return new SuccessResult("Subcategory added successfuly");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 547)
                    {
                        return new ErrorResult("Kategori bulunamadı", "BadRequest");
                    }
                    else if (sqlEx.Number == 2627)
                    {
                        return new ErrorResult("Aynı isme sahip alt kategori mevcut.", "BadRequest");
                    }
                }
                var errorDetails = new
                {
                    Message = "CategoryId does not exist in the Categories table.",
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "BadRequest");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IResult> DeleteSubcategoryAsync(int id)
        {
            Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == id).ConfigureAwait(false);
            if (subcategory == null)
            {
                return new ErrorResult("Alt kategori bulunamadı", "NotFound");
            }

            if (subcategory.Products != null && subcategory.Products.Count != 0)
            {
                foreach (var product in subcategory.Products)
                {
                    if (product.OrderProducts.Count != 0)
                    {
                        return new ErrorResult("Bu kategorideki ürünlerin daha tamamlanmamış siparişleri var. Kategoriyi silebilmek için siparişlerin tamalanmasını bekleyin", "BadRequest");
                    }
                }
            }

            try
            {
                if (subcategory.ImageUrl != null)
                {
                    var key = subcategory.ImageUrl.Substring(subcategory.ImageUrl.LastIndexOf('/') + 1);
                    var result = await _awsS3Manager.DeleteFileAsync(key);
                    if (!result.Success)
                    {
                        return new ErrorResult(result.Message, "SystemError");
                    }
                }

                await _unitOfWork.Subcategories.DeleteAsync(subcategory).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                return new SuccessResult("Alt kategori silindi.");
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

        public async Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesAsync()
        {
            try
            {
                IEnumerable<Subcategory> subcategories = await _unitOfWork.Subcategories.GetAllAsync().ConfigureAwait(false);
                if (subcategories == null || !subcategories.Any())
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>(Array.Empty<SubcategoryDto>(), "Alt kategori bulunamadı!");
                }
                IEnumerable<SubcategoryDto> subcategoryDtos = subcategories.Select(s => new SubcategoryDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    ImageUrl = s.ImageUrl                    
                });
                return new SuccessDataResult<IEnumerable<SubcategoryDto>>(subcategoryDtos);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<SubcategoryDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesByCategoryIdAsync(int categoryId)
        {
            try
            {
                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId).ConfigureAwait(false);
                if (category == null)
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Subcategory> subcategories = await _unitOfWork.Subcategories.GetSubcategoriesByCategoryId(categoryId).ConfigureAwait(false);
                if (subcategories == null || !subcategories.Any())
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>(Array.Empty<SubcategoryDto>(), "Alt kategori bulunamdı!");
                }
                IEnumerable<SubcategoryDto> subcategoryDtos = subcategories.Select(s => new SubcategoryDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                });
                return new SuccessDataResult<IEnumerable<SubcategoryDto>>(subcategoryDtos);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<SubcategoryDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<IDataResult<SubcategoryDto>> GetSubcategoryByIdAsync(int subcategoryId)
        {
            try
            {
                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryId).ConfigureAwait(false);
                if (subcategory == null)
                {
                    return new ErrorDataResult<SubcategoryDto>("Aradığınız alt kategori bulunamadı!", "NotFound");
                }
                SubcategoryDto subcategoryDto = new SubcategoryDto()
                {
                    Id = subcategory.Id,
                    Name = subcategory.Name,
                    CategoryId = subcategory.CategoryId,
                    ImageUrl = subcategory.ImageUrl
                };

                return new SuccessDataResult<SubcategoryDto>(subcategoryDto);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<SubcategoryDto>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<IResult> UpdateSubcategoryAsync(SubcategoryCreateDto subcategoryCreateDto)
        {
            Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(subcategory => subcategoryCreateDto.Id == subcategory.Id).ConfigureAwait(false);
            if (subcategory == null)
            {
                return new ErrorResult("Alt kategori bulunamadı", "NotFound");
            }

            var validator = new SubcategoryCreateDtoValidator();
            var validationResults = await validator.ValidateAsync(subcategoryCreateDto).ConfigureAwait(false);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                if (errorMessage != null)
                {
                    return new ErrorResult(errorMessage, "BadRequest");
                }
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                subcategory.Name = subcategoryCreateDto.Name;
                subcategory.CategoryId = subcategoryCreateDto.CategoryId;

                if (subcategoryCreateDto.Image != null && subcategoryCreateDto.Image.Length > 0)
                {
                    var fileUrl = await _awsS3Manager.UploadFileAsync(subcategoryCreateDto.Image);
                    if (!fileUrl.Success)
                    {
                        return new ErrorResult(fileUrl.Message, "SystemError");
                    }
                    subcategory.ImageUrl = fileUrl.Data;
                }
                else if (subcategoryCreateDto.Image != null && subcategoryCreateDto.Image.Length == 0 && subcategory.ImageUrl != null)
                {
                    var key = subcategory.ImageUrl.Substring(subcategory.ImageUrl.LastIndexOf('/') + 1);
                    var result = await _awsS3Manager.DeleteFileAsync(key);
                    if (!result.Success)
                    {
                        return new ErrorResult(result.Message, "SystemError");
                    }
                    subcategory.ImageUrl = null;
                }


                await _unitOfWork.Subcategories.UpdateAsync(subcategory).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);
                return new SuccessResult("Alt kategori güncellendi.");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 547)
                    {
                        return new ErrorResult("Kategori bulunamadı", "BadRequest");
                    }
                    else if (sqlEx.Number == 2627)
                    {
                        return new ErrorResult("Aynı isme sahip alt kategori mevcut.", "BadRequest");
                    }
                }
                var errorDetails = new
                {
                    Message = "CategoryId does not exist in the Categories table.",
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "BadRequest");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
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
