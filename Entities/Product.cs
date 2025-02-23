using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string? Size { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set;}
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public Category Category { get; set; }

        [JsonIgnore]
        public ICollection<OrderProduct> OrderProducts { get; set; }
        public ICollection<Image> Images { get; set; }

        [JsonIgnore]
        public ICollection<CartItem> CartItems { get; set; }
        public int SubcategoryId { get; set; }

        [JsonIgnore]
        public Subcategory Subcategory { get; set; }
    }
}
