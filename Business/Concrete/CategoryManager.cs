﻿using Business.Abstract;
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
    public class CategoryManager : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<CategoryCreateDto>> AddAsync(CategoryCreateDto categoryCreateDto)
        {
            Category category = await _unitOfWork.Categories.GetAsync(c => c.Name == categoryCreateDto.Name);
            if (category == null)
            {
                Category newCategory = new Category()
                {
                    Name = categoryCreateDto.Name
                };
                await _unitOfWork.Categories.AddAsync(newCategory);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessDataResult<CategoryCreateDto>(categoryCreateDto,"Category added succesfully!");
            }
            return new ErrorDataResult<CategoryCreateDto>("This category added before!");
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == id);
            if (category != null)
            {
                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessResult("Category Deleted Suucesfully!");
            }

            return new ErrorResult("Category not found!");
        }

        public async Task<IDataResult<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            IEnumerable<Category> categories = await _unitOfWork.Categories.GetAllAsync();
            if(categories == null)
            {
                return new ErrorDataResult<IEnumerable<CategoryDto>>("Kategori bulunamdı!");
            }
            IEnumerable<CategoryDto> categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });

            return new SuccessDataResult<IEnumerable<CategoryDto>>(categoryDtos);
        }

        public async Task<IDataResult<CategoryDto>> GetByIdAsync(int id)
        {
            Category category = await _unitOfWork.Categories.GetAsync(c => c.Id==id);
            if(category != null)
            {
                CategoryDto categoryDto = new CategoryDto()
                {
                    Id=category.Id,
                    Name = category.Name
                };
                return new SuccessDataResult<CategoryDto>(categoryDto);
            }
            return new ErrorDataResult<CategoryDto>("Aradığınız kategori bulunamadı!");
        }

        public async Task<IResult> UpdateAsync(CategoryUpdateDto categoryUpdateDto)
        {
            Category category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryUpdateDto.Id);
            if(category != null)
            {
                category.Name = categoryUpdateDto.Name;
                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessDataResult<CategoryUpdateDto>(categoryUpdateDto,"Category updated successfully!");
            }
            return new ErrorResult("Category could not be updated!");
        }
    }
}