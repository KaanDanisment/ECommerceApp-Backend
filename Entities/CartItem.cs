using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class CartItem: IEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Quantity { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("Cart")]
        public string CartId { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; }
        public Product Product { get; set; }
        public Cart Cart { get; set; }
    }
}
