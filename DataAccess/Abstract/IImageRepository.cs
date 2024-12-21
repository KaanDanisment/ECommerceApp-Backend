using Core.DataAccess.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IImageRepository : IGenericRepository<Image>
    {
        Task<IEnumerable<Image>> GetImagesByProductIdAsync(int ProductId);
        Task AddRangeAsync(IEnumerable<Image> images);
        Task DeleteByProductIdAsync(int productId);
    }
}
