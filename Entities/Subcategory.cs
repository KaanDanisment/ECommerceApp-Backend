using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Subcategory : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public Category Category { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
