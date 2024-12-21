using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
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

        public SubcategoryManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<SubcategoryCreateDto>> AddSubcategoryAsync(SubcategoryCreateDto subcategoryCreateDto)
        {
            Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Name == subcategoryCreateDto.Name);
            if (subcategory != null)
            {
                return new ErrorDataResult<SubcategoryCreateDto>("This subcategory added before");
            }
            Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == subcategoryCreateDto.CategoryId);
            if(category == null)
            {
                return new ErrorDataResult<SubcategoryCreateDto>("Wrong category selection");
            }

            Subcategory newSubcategory = new Subcategory()
            {
                Name = subcategoryCreateDto.Name,
                CategoryId = subcategoryCreateDto.CategoryId,
                ImageUrl = subcategoryCreateDto.ImageUrl,
            };
            await _unitOfWork.Subcategories.AddAsync(newSubcategory);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessDataResult<SubcategoryCreateDto>(subcategoryCreateDto,"Subcategory added successfuly");
        }

        public async Task<IResult> DeleteSubcategoryAsync(int id)
        {
            Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == id);
            if (subcategory != null)
            {
                await _unitOfWork.Subcategories.DeleteAsync(subcategory);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Subcategory deleted successfuly");
            }
            return new ErrorResult("Subcategory was not found");
        }

        public async Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesAsync()
        {
            try
            {
                IEnumerable<Subcategory> subcategories = await _unitOfWork.Subcategories.GetAllAsync();
                if (subcategories == null || !subcategories.Any())
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>([], "Alt kategori bulunamadı!");
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

        public async Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesByCategoryIdAsync(int categoryId)
        {
            try
            {
                Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
                if (category == null)
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>("Aradığınız kategori bulunamadı!", "NotFound");
                }
                IEnumerable<Subcategory> subcategories = await _unitOfWork.Subcategories.GetSubcategoriesByCategoryId(categoryId);
                if (subcategories == null)
                {
                    return new ErrorDataResult<IEnumerable<SubcategoryDto>>([], "Alt kategori bulunamdı!");
                }
                IEnumerable<SubcategoryDto> subcategoryDtos = subcategories.Select(s => new SubcategoryDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                });
                return new SuccessDataResult<IEnumerable<SubcategoryDto>>(subcategoryDtos);
            }
            catch(Exception ex)
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
                Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryId);
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
            catch(Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<SubcategoryDto>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            
        }

        public async Task<IResult> UpdateSubcategoryAsync(SubcategoryUpdateDto subcategoryUpdateDto)
        {
            Subcategory subcategory = await _unitOfWork.Subcategories.GetAsync(s => s.Id == subcategoryUpdateDto.Id);
            if(subcategory != null)
            {
                subcategory.Name = subcategoryUpdateDto.Name;
                subcategory.CategoryId = subcategoryUpdateDto.CategoryId;
                subcategory.ImageUrl = subcategoryUpdateDto.ImageUrl;

                await _unitOfWork.Subcategories.UpdateAsync(subcategory);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessResult("Subcategory updated succesfuly");
            }
            return new ErrorResult("SubCategory was not found");
        }
    }
}
