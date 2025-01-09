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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AdminManager : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenHelper _tokenHelper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly RefreshTokenHelper _refreshTokenHelper;
        public AdminManager(IUnitOfWork unitOfWork, ITokenHelper tokenHelper, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, RefreshTokenHelper refreshTokenHelper)
        {
            _unitOfWork = unitOfWork;
            _tokenHelper = tokenHelper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _refreshTokenHelper = refreshTokenHelper;
        }
        
        public async Task<IDataResult<User>> AdminLoginAsync(AdminLoginDto adminLoginDto)
        {
            var validator = new AdminLoginDtoValidator();
            var validationResults = await validator.ValidateAsync(adminLoginDto).ConfigureAwait(false);

            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                return new ErrorDataResult<User>(errorMessage, "BadRequest");
            }
            try
            {
                User user = await _userManager.FindByEmailAsync(adminLoginDto.Email).ConfigureAwait(false);
                if (user == null)
                {
                    return new ErrorDataResult<User>("Mail adresi kayıtlı değil! Lütfen kayıt olun.", "NotFound");
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, adminLoginDto.Password, false).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return new ErrorDataResult<User>("Hatalı şifre! Lütfen tekrar deneyin.", "BadRequest");
                }
                return new SuccessDataResult<User>(user,"Giriş başarılı!");
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
        }

        public async Task<IResult> AdminRegisterAsync(AdminRegisterDto adminRegisterDto)
        {
            var validator = new AdminRegisterDtoValidator();
            var validationResults = await validator.ValidateAsync(adminRegisterDto).ConfigureAwait(false);
            if (!validationResults.IsValid)
            {
                var errorMessage = validationResults.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                return new ErrorResult(errorMessage,"BadRequest");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                User user = new User()
                {
                    Email = adminRegisterDto.Email,
                    UserName = adminRegisterDto.Email,
                    FirstName = adminRegisterDto.FirstName,
                    LastName = adminRegisterDto.LastName,
                    PhoneNumber = adminRegisterDto.PhoneNumber
                };
                var identityResult = await _userManager.CreateAsync(user, adminRegisterDto.Password).ConfigureAwait(false);
                if (!identityResult.Succeeded)
                {
                    var identityError = identityResult.Errors.FirstOrDefault();
                    if (identityError != null)
                    {
                        return new ErrorResult(identityError.Description, "BadRequest");
                    }
                }

                if (!await _roleManager.RoleExistsAsync("Admin").ConfigureAwait(false))
                {
                    await _roleManager.CreateAsync(new Role { Name = "Admin" }).ConfigureAwait(false);
                }
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin").ConfigureAwait(false);
                
                await transaction.CommitAsync().ConfigureAwait(false);

                return new SuccessResult("Admin oluşturma başarılı!");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (InvalidOperationException InvalidEx)
            {
                var errorDetails = new
                {
                    Message = InvalidEx.Message,
                    InnerException = InvalidEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<AccessToken>> CreateJwtTokenAsync(User user)
        {
            try
            {
                var token = await _tokenHelper.CreateTokenAsync(user).ConfigureAwait(false);

                return new SuccessDataResult<AccessToken>(token, "Token created successfuly!");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<AccessToken>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<string>> CreateRefreshTokenAsync(User user)
        {
            try
            {
                var token = await _refreshTokenHelper.GenerateAndStoreRefreshTokenAsync(user).ConfigureAwait(false);

                return new SuccessDataResult<string>(token, "Token created");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<string>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<AdminDto>> GetAdminInfos(User user)
        {
            AdminDto adminDto = new AdminDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return new SuccessDataResult<AdminDto>(adminDto);
        }

        public async Task<IDataResult<User>> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var userToken = await _unitOfWork.UserTokens.GetAsync(t =>
                       t.LoginProvider == "CustomRefreshToken" &&
                       t.Name == "RefreshToken" &&
                       t.Value == refreshToken).ConfigureAwait(false);

                if (userToken == null)
                {
                    return new ErrorDataResult<User>("Invalid or expired refresh token");
                }

                User user = await _userManager.FindByIdAsync(userToken.UserId).ConfigureAwait(false);
                if (user == null)
                {
                    return new ErrorDataResult<User>("Invalid or expired refresh token");
                }
                return new SuccessDataResult<User>(user);
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

        }

        public async Task<IResult> UserExist(string email)
        {
            try
            {
                if (await _userManager.Users.AnyAsync(u => u.Email == email).ConfigureAwait(false))
                {
                    return new ErrorResult("Bu mail adresi kayıtlı! Giriş yapın veya başka bir mail adresi ile kayıt olun.");
                }
                return new SuccessResult();
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails));
            }
        }
    }
}
