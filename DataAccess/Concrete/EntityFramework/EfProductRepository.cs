using Core.DataAccess;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Core.Concrete.EntityFramework;
using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfProductRepository : EfGenericRepositoryBase<Product, ECommerceContext>, IProductRepository
    {
        private readonly ECommerceContext _context;
        public EfProductRepository(ECommerceContext context) : base(context)
        {
            _context = context;
        }

        public Task<IEnumerable<Product>> GetBestSellerProducts()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Product>> GetLatestProducts()
        {
            return await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Product>> GetProductsBySubcategoryIdAsync(int subcategoryId)
        {
            return await _context.Products
                .Where(p => p.SubcategoryId == subcategoryId)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
