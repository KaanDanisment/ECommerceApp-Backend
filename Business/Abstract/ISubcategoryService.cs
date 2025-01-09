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
    public interface ISubcategoryService
    {
        Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesAsync();
        Task<IDataResult<IEnumerable<SubcategoryDto>>> GetAllSubcategoriesByCategoryIdAsync(int categoryId);
        Task<IDataResult<SubcategoryDto>> GetSubcategoryByIdAsync(int subcategoryId);
        Task<IResult> AddAsync(SubcategoryCreateDto subcategoryCreateDto);
        Task<IResult> DeleteSubcategoryAsync(int id);
        Task<IResult> UpdateSubcategoryAsync(SubcategoryCreateDto subcategoryUpdateDto);
    }
}
