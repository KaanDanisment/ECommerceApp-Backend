using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using Core.Utilities.Security.jwt;
using Core.Utilities.Security.RefreshToken;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using Entities.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenHelper _tokenHelper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly RefreshTokenHelper _refreshTokenHelper;

        public AuthManager(IUnitOfWork unitOfWork, ITokenHelper tokenHelper, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, RefreshTokenHelper refreshTokenHelper)
        {
            _unitOfWork = unitOfWork;
            _tokenHelper = tokenHelper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _refreshTokenHelper = refreshTokenHelper;
        }

        public async Task<IDataResult<AccessToken>> CreateJwtTokenAsync(User user)
        {
            var token = await _tokenHelper.CreateTokenAsync(user);
            if (token == null)
            {
                return new ErrorDataResult<AccessToken>("Token could not created");
            }
            return new SuccessDataResult<AccessToken>(token, "Token created successfuly!");
        }

        public async Task<IDataResult<string>> CreateRefreshTokenAsync(User user)
        {
            var token = await _refreshTokenHelper.GenerateAndStoreRefreshTokenAsync(user);
            if (token == null)
            {
                return new ErrorDataResult<string>("Refresh Token could not created");
            }
            return new SuccessDataResult<string>(token, "Token created");
        }

        public async Task<IDataResult<User>> LoginAsync(UserLoginDto userLoginDto)
        {
            var validator = new UserLoginDtoValidator();
            var validationResult = await validator.ValidateAsync(userLoginDto);
            if (!validationResult.IsValid)
            {
                var validationError = validationResult.Errors.FirstOrDefault();
                if (validationError != null)
                {
                    return new ErrorDataResult<User>(validationError.ErrorMessage, "BadRequest");
                }
            }
            var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
            if (user == null) { return new ErrorDataResult<User>("Mail adresi kayıtlı değil! Lütfen kayıt olun.", "BadRequest"); }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDto.Password, false);
            if (!result.Succeeded) { return new ErrorDataResult<User>("Hatalı şifre! Lütfen tekrar deneyin.", "BadRequest"); }

            try
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<User>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

            return new SuccessDataResult<User>(user, "Successful login");
        }

        public async Task<IDataResult<UserRegisterDto>> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                var validator = new UserRegisterDtoValidator();
                var validationResult = await validator.ValidateAsync(userRegisterDto);
                if (!validationResult.IsValid)
                {
                    var valiadaitonError = validationResult.Errors.FirstOrDefault();
                    if (valiadaitonError != null)
                    {
                        return new ErrorDataResult<UserRegisterDto>(valiadaitonError.ErrorMessage, "BadRequest");
                    }
                }

                User user = new User()
                {
                    FirstName = userRegisterDto.FirstName,
                    LastName = userRegisterDto.LastName,
                    UserName = userRegisterDto.Email,
                    Email = userRegisterDto.Email,
                    PhoneNumber = userRegisterDto.PhoneNumber,
                };

                var identityResult = await _userManager.CreateAsync(user, userRegisterDto.Password);
                if (!identityResult.Succeeded)
                {
                    var identityError = identityResult.Errors.FirstOrDefault();
                    if (identityError != null)
                    {
                        return new ErrorDataResult<UserRegisterDto>(identityError.Description, "BadRequest");
                    }
                }

                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    var role = new Role { Name = "User" };
                    var createRoleResult = await _roleManager.CreateAsync(role);
                    if (!createRoleResult.Succeeded)
                    {
                        var errorDetails = createRoleResult.Errors.Select(e => new { e.Code, e.Description });
                        var errorMessages = System.Text.Json.JsonSerializer.Serialize(errorDetails);
                        return new ErrorDataResult<UserRegisterDto>(errorMessages, "SystemError");
                    }
                }

                var addRoleToUser = await _userManager.AddToRoleAsync(user, "User");
                if (!addRoleToUser.Succeeded)
                {
                    var errorDetails = string.Join(", ", addRoleToUser.Errors.Select(e => e.Description));
                    var roleAssignmentErrors = System.Text.Json.JsonSerializer.Serialize(errorDetails);
                    return new ErrorDataResult<UserRegisterDto>(roleAssignmentErrors, "SystemError");
                }

                await transaction.CommitAsync();

                return new SuccessDataResult<UserRegisterDto>(userRegisterDto, "User created successfuly");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorDataResult<UserRegisterDto>(userRegisterDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<UserRegisterDto>(userRegisterDto, System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<IResult> UserExist(string email)
        {
            if (await _userManager.Users.AnyAsync(user => user.Email == email))
            {
                return new ErrorResult("Bu mail adresi kayıtlı! Giriş yapın veya başka bir mail adresi ile kayıt olun.");
            }
            return new SuccessResult();
        }

        public async Task<IDataResult<User>> ValidateRefreshTokenAsync(string refreshToken)
        {
            var userToken = await _unitOfWork.UserTokens.GetAsync(t =>

               t.LoginProvider == "CustomRefreshToken" &&
               t.Name == "RefreshToken" &&
               t.Value == refreshToken);
            if (userToken == null)
            {
                return new ErrorDataResult<User>("Invalid or expired refresh token");
            }

            User user = await _userManager.FindByIdAsync(userToken.UserId);
            if (user == null)
            {
                return new ErrorDataResult<User>("Invalid or expired refresh token");
            }
            return new SuccessDataResult<User>(user);
        }
    }
}
