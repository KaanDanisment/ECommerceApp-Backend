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
    public class EfSubcategoryRepository : EfGenericRepositoryBase<Subcategory, ECommerceContext>, ISubcategoryRepository
    {
        private readonly ECommerceContext _context;
        public EfSubcategoryRepository(ECommerceContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subcategory>> GetSubcategoriesByCategoryId(int categoryId)
        {
            return await _context.Subcategories
                .Where(subcategory => subcategory.CategoryId == categoryId)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
