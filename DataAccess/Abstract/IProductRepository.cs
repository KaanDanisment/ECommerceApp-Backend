using Core.DataAccess.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId, string? orderBy = null);
        Task<IEnumerable<Product>> GetProductsBySubcategoryIdAsync(int subcategoryId, string? orderBy = null);
        Task<IEnumerable<Product>> GetLatestProducts();
    }
}
