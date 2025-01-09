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
    public interface IAuthService
    {
        Task<IDataResult<User>> LoginAsync(UserLoginDto userLoginDto);
        Task<IResult> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<IResult> UserExist(string email);
        Task<IDataResult<AccessToken>> CreateJwtTokenAsync(User user);
        Task<IDataResult<string>> CreateRefreshTokenAsync(User user);
        Task<IDataResult<User>> ValidateRefreshTokenAsync(string refreshToken);
    }
}
