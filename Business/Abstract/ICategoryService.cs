using Core.Utilities.Results.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICategoryService
    {
        Task<IDataResult<CategoryDto>> GetByIdAsync(int id);
        Task<IDataResult<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<IDataResult<CategoryCreateDto>> AddAsync(CategoryCreateDto categoryCreateDto);
        Task<IResult> UpdateAsync(CategoryUpdateDto categoryUpdateDto);
        Task<IResult> DeleteAsync(int id);
    }
}
