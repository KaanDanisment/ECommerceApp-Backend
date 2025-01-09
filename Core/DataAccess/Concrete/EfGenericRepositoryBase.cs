using Core.DataAccess.Abstract;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Core.Concrete.EntityFramework
{
    public class EfGenericRepositoryBase<TEntity, TContext> : IGenericRepository<TEntity>
            where TEntity : class, new()
            where TContext : DbContext
    {
        private readonly TContext _context;

        public EfGenericRepositoryBase(TContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
                ? await _context.Set<TEntity>().ToListAsync().ConfigureAwait(false)
                : await _context.Set<TEntity>().Where(filter).ToListAsync().ConfigureAwait(false);
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _context.Set<TEntity>().SingleOrDefaultAsync(filter).ConfigureAwait(false);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
