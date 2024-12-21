using Core.DataAccess;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Core.Concrete.EntityFramework;
using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfCategoryRepository : EfGenericRepositoryBase<Category,ECommerceContext>, ICategoryRepository
    {
        public EfCategoryRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
