using Core.Entities.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Core.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfAddressRepository : EfGenericRepositoryBase<Address, ECommerceContext>, IAddressRepository
    {
        public EfAddressRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
