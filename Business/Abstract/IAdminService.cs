using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Security.jwt;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAdminService
    {
        Task<IDataResult<User>> AdminLoginAsync(AdminLoginDto adminLoginDto);
        Task<IResult> AdminRegisterAsync(AdminRegisterDto adminRegisterDto);
        Task<IDataResult<AccessToken>> CreateJwtTokenAsync(User user);
        Task<IDataResult<string>> CreateRefreshTokenAsync(User user);
        Task<IDataResult<AdminDto>> GetAdminInfos(User user);
        Task<IDataResult<User>> ValidateRefreshTokenAsync(string refreshToken);
        Task<IResult> UserExist(string email);
    }
}
