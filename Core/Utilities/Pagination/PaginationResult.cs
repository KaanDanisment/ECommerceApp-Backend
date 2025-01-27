using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Pagination
{
    public class PaginationResult<T>
    {
        public IEnumerable<T> Items { get; }
        public int TotalItems { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        public PaginationResult(IEnumerable<T> items, int totalItems, int page, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
        }
    }
}
