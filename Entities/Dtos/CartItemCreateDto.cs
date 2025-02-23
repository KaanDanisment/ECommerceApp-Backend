using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CartItemCreateDto
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
    }
}
