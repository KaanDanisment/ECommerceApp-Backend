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
    public class EfOrderRepository : EfGenericRepositoryBase<Order,ECommerceContext>, IOrderRepository
    {
        public EfOrderRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
