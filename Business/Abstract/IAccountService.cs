using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAccountService
    {
        Task<IDataResult<UserDto>> GetUserInfos(User user);
        Task<IResult> CreateAddressAsync(AddressDto addressDto,User user);
        Task<IDataResult<IEnumerable<AddressDto>>> GetUsersAdressesAsync(User user);
        Task<IResult> UpdateUserAsync(User user,UserDto userDto);
        Task<IResult> DeleteAddressAsync(AddressDto addressDto);
    }
}
