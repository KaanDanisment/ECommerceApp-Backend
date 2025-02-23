using Core.Utilities.GroupedProductsResult;
using Core.Utilities.Pagination;
using Core.Utilities.Results.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IProductService
    {
        Task<IDataResult<ProductDto>> GetByIdAsync(int id);
        Task<IDataResult<PaginationResult<ProductDto>>> GetAllAsync(int? page, string? sortBy);
        Task<IDataResult<ProductCreateDto>> AddAsync(ProductCreateDto productCreateDto);
        Task<IResult> UpdateAsync(ProductUpdateDto productUpdateDto);
        Task<IResult> DeleteAsync(int id);
        Task<IDataResult<IEnumerable<ProductDto>>> GetProductsByCategoryId(int categoryId, string? sortBy);
        Task<IDataResult<IEnumerable<ProductDto>>> GetProductsBySubcategoryId(int subcategoryId, string? sortBy);
        Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetLatestProductsAsync();
        Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetGroupedProductsByCategoryId(int categoryId, string? sortBy);
        Task<IDataResult<IEnumerable<GroupedProductsResult<ProductDto>>>> GetGroupedProductsBySubcategoryId(int subcategoryId, string? sortBy);
        Task<IDataResult<GroupedProductsResult<ProductDto>>> GetGroupedProductByProductId(int productId);
    }
}
