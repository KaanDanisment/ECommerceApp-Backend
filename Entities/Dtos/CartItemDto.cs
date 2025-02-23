using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CartItemDto
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public string CartId { get; set; }
        public decimal UnitPrice { get; set; }
        public ProductDto ProductDto { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
