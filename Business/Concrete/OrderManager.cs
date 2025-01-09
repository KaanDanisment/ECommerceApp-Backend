using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class OrderManager : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<OrderCreateDto>> CreateAsync(OrderCreateDto orderCreateDto)
        {
            Order order = new Order()
            {
                UserId = orderCreateDto.UserdId,
                TotalPrice = orderCreateDto.UnitPrice,
            };
            await _unitOfWork.Orders.AddAsync(order).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            return new SuccessDataResult<OrderCreateDto>(orderCreateDto, "Order created succsesfully!");
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            Order order = await _unitOfWork.Orders.GetAsync(o => o.Id == id).ConfigureAwait(false);
            if (order != null)
            {
                await _unitOfWork.Orders.DeleteAsync(order).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                return new SuccessResult("Order deleted successfuly!");
            }
            return new ErrorResult("Order not found!");
        }

        public async Task<IDataResult<IEnumerable<OrderDto>>> GetAllAsync()
        {
            IEnumerable<Order> orders = await _unitOfWork.Orders.GetAllAsync().ConfigureAwait(false);
            IEnumerable<OrderDto> orderDtos = orders.Select(o => new OrderDto()
            {
                OrderId = o.Id,
                UserId = o.UserId,
                UnitPrice = o.TotalPrice,
                OrderDate = o.OrderDate,
                Status = o.Status
            });
            return new SuccessDataResult<IEnumerable<OrderDto>>(orderDtos);
        }

        public async Task<IDataResult<OrderDto>> GetByIdAsync(int id)
        {
            Order order = await _unitOfWork.Orders.GetAsync(o => o.Id == id).ConfigureAwait(false);
            if (order != null)
            {
                OrderDto orderDo = new OrderDto()
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    UnitPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status
                };
                return new SuccessDataResult<OrderDto>(orderDo);
            }
            return new ErrorDataResult<OrderDto>("Order not found");
        }

        public async Task<IResult> UpdateOrderStatusAsync(int orderId, string status)
        {
            Order order = await _unitOfWork.Orders.GetAsync(o => orderId == o.Id).ConfigureAwait(false);
            if (order != null)
            {
                order.Status = status;
                await _unitOfWork.Orders.UpdateAsync(order).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                return new SuccessResult("Status updated successfuly");
            }
            return new ErrorResult("Order not found");
        }
    }
}
