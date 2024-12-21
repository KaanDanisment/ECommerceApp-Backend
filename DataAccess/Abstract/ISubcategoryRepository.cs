using Core.DataAccess.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface ISubcategoryRepository : IGenericRepository<Subcategory>
    {
        Task<IEnumerable<Subcategory>> GetSubcategoriesByCategoryId(int categoryId);
    }
}
