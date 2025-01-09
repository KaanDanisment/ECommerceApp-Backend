using Core.Utilities.Results.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IOrderService
    {
        Task<IDataResult<OrderDto>> GetByIdAsync(int id);
        Task<IDataResult<IEnumerable<OrderDto>>> GetAllAsync();
        Task<IDataResult<OrderCreateDto>> CreateAsync(OrderCreateDto orderCreateDto);
        Task<IResult> DeleteAsync(int id);
        Task<IResult> UpdateOrderStatusAsync(int orderId,string status);
    }
}
