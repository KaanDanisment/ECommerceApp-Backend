using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.DataAccess.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IImageRepository Images { get; }
        ISubcategoryRepository Subcategories { get; }
        IOrderProductRepository OrderProducts { get; }
        IAddressRepository Addresses { get; }
        ICartRespository Carts { get; }
        ICartItemRepository CartItems { get; }
        IGenericRepository<IdentityUserToken<string>> UserTokens { get; }
        
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
