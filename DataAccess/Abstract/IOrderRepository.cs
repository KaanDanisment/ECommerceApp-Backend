using Core.DataAccess.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
    }
}
