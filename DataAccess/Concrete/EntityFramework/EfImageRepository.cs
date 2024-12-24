using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Core.Concrete.EntityFramework;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfImageRepository : EfGenericRepositoryBase<Image, ECommerceContext>, IImageRepository
    {
        private readonly ECommerceContext _context;
        public EfImageRepository(ECommerceContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Image> images)
        {
            await _context.Images.AddRangeAsync(images).ConfigureAwait(false);
        }

        public async Task DeleteByProductIdAsync(int productId)
        {
            IEnumerable<Image> images = await _context.Images
                .Where(image => image.ProductId == productId)
                .ToListAsync()
                .ConfigureAwait(false);
            _context.Images.RemoveRange(images);
        }

        public async Task<IEnumerable<Image>> GetImagesByProductIdAsync(int ProductId)
        {
            return await _context.Images
                .Where(image => image.ProductId == ProductId)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
