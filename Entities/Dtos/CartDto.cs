using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CartDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public ICollection<CartItemDto> CartItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
