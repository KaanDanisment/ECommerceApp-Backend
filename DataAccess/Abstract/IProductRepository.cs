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
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsBySubcategoryIdAsync(int subcategoryId);
        Task<IEnumerable<Product>> GetLatestProducts();
        Task<IEnumerable<Product>> GetBestSellerProducts();
    }
}
