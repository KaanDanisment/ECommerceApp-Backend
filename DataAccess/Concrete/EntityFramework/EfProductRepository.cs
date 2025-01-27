using Core.DataAccess;
using Core.Utilities.Results.Abstract;
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

        public async Task<IEnumerable<Product>> GetLatestProducts()
        {
            return await _context.Products
                .Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId, string? orderBy = null)
        {
            if (orderBy != null)
            {
                var orderedProducts = SortProducts(_context.Products.Where(p => p.CategoryId == categoryId), orderBy);
                return await orderedProducts
                    .Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            return await _context.Products
                .Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Product>> GetProductsBySubcategoryIdAsync(int subcategoryId, string sortBy)
        {
            if (sortBy != null)
            {
                var orderedProducts = SortProducts(_context.Products.Where(p => p.SubcategoryId == subcategoryId), sortBy);
                return await orderedProducts
                    .Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            return await _context.Products
                .Include(p => p.Images).Include(p => p.Category).Include(p => p.Subcategory)
                .Where(p => p.SubcategoryId == subcategoryId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private IQueryable<Product> SortProducts(IQueryable<Product> products, string sortBy)
        {
            return sortBy switch
            {
                "id_descending" => products.OrderByDescending(p => p.Id),
                "price_ascending" => products.OrderBy(p => p.Price),
                "price_descending" => products.OrderByDescending(p => p.Price),
                "stock_ascending" => products.OrderBy(p => p.Stock),
                "stock_descending" => products.OrderByDescending(p => p.Stock),
                "date_ascending" => products.OrderBy(p => p.CreatedAt),
                "date_descending" => products.OrderByDescending(p => p.CreatedAt),
                _ => products.OrderBy(p => p.Id),
            };
        }
    }
}
