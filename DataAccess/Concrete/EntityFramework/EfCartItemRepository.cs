using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Core.Concrete.EntityFramework;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfCartItemRepository : EfGenericRepositoryBase<CartItem, ECommerceContext>, ICartItemRepository
    {
        public EfCartItemRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
