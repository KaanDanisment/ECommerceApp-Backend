using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.GroupedProductsResult
{
    public class GroupedProductsResult<T>
    {
        public IEnumerable<T> Products { get; }
        public string Name { get; }
        public string Color { get; set; }
        public IEnumerable<string> ImageUrls { get; }
        public IEnumerable<string>? Sizes{ get; }
        public string Description { get; }
        public decimal Price { get; }

        public GroupedProductsResult(IEnumerable<T> products)
        {
            Products = products;
            var firstProduct = products.First();
            Name = firstProduct.GetType().GetProperty("Name")?.GetValue(firstProduct)?.ToString() ?? string.Empty;
            Color = firstProduct.GetType().GetProperty("Color")?.GetValue(firstProduct)?.ToString() ?? string.Empty;
            var imageUrlsObj = firstProduct.GetType().GetProperty("ImageUrls")?.GetValue(firstProduct);
            ImageUrls = imageUrlsObj as IEnumerable<string>;
            Sizes = products.Select(p => p.GetType().GetProperty("Size")?.GetValue(p)?.ToString()).Where(size => size != null).ToList();
            Description = firstProduct.GetType().GetProperty("Description")?.GetValue(firstProduct)?.ToString() ?? string.Empty;
            Price = Convert.ToDecimal(firstProduct.GetType().GetProperty("Price")?.GetValue(firstProduct));
        }
    }
}
