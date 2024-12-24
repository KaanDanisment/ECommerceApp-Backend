using Core.DataAccess;
using Core.DataAccess.Abstract;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ECommerceContext _context;
        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public IImageRepository Images { get; }
        public ISubcategoryRepository Subcategories { get; }
        public IOrderProductRepository OrderProducts { get; }
        public IAddressRepository Addresses { get; }
        public IGenericRepository<IdentityUserToken<string>> UserTokens { get; }

        public UnitOfWork(ECommerceContext context, ICategoryRepository categoryRepository, IProductRepository productRepository, IOrderRepository orderRepository, IImageRepository images, ISubcategoryRepository subcategories, IGenericRepository<IdentityUserToken<string>> userTokens, IOrderProductRepository orderProducts, IAddressRepository addresses)
        {
            _context = context;
            Categories = categoryRepository;
            Products = productRepository;
            Orders = orderRepository;
            Images = images;
            Subcategories = subcategories;
            UserTokens = userTokens;
            OrderProducts = orderProducts;
            Addresses = addresses;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
        }
    }
}
