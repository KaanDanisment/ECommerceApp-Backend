using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OrderCreateDto
    {
        public string UserdId { get; set; }
        public decimal UnitPrice { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public List<OrderProduct> Products { get; set; }
    }
}