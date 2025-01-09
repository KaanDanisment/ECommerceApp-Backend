using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities.Dtos;
using Entities.Validators;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AccountManager : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public AccountManager(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IDataResult<UserDto>> GetUserInfos(User user)
        {
            UserDto userDto = new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            var result = await this.GetUsersAdressesAsync(user);
            if (!result.Success && result.Data == null)
            {
                return new ErrorDataResult<UserDto>(result.Message);
            }
            else
            {
                userDto.Addresses = result.Data;
                return new SuccessDataResult<UserDto>(userDto);
            }
        }

        public async Task<IResult> CreateAddressAsync(AddressDto addressDto, User user)
        {
            var validator = new AddressDtoValidator();
            var validationResult = await validator.ValidateAsync(addressDto).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var errorMessage = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                return new ErrorResult(errorMessage, "BadRequest");
            }
            try
            {
                Address address = new Address()
                {
                    AddressLine = addressDto.AddressLine,
                    AddressLine2 = addressDto.AddressLine2,
                    City = addressDto.City,
                    Country = addressDto.Country,
                    UserId = user.Id
                };
                await _unitOfWork.Addresses.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Adres başarıyla eklendi");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IDataResult<IEnumerable<AddressDto>>> GetUsersAdressesAsync(User user)
        {
            try
            {
                IEnumerable<Address> addresses = await _unitOfWork.Addresses.GetAllAsync(a => a.UserId == user.Id);
                if (addresses == null || !addresses.Any())
                {
                    return new ErrorDataResult<IEnumerable<AddressDto>>([], "Adres listesi boş");
                }
                IEnumerable<AddressDto> addressDtos = addresses.Select(a => new AddressDto()
                {
                    Id = a.Id,
                    AddressLine = a.AddressLine,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    Country = a.Country,
                });

                return new SuccessDataResult<IEnumerable<AddressDto>>(addressDtos);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<IEnumerable<AddressDto>>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IResult> UpdateUserAsync(User user, UserDto userDto)
        {
            var validator = new UserDtoValidator();
            var validationResult = await validator.ValidateAsync(userDto).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var errorMessage = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                return new ErrorDataResult<UserRegisterDto>(errorMessage, "BadRequest");

            }
            try
            {
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.UserName = userDto.Email;
                user.PhoneNumber = userDto.PhoneNumber;

                var identityResult = await _userManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                {
                    var identityError = identityResult.Errors.FirstOrDefault();
                    if (identityError != null)
                    {
                        return new ErrorResult(identityError.Description, "BadRequest");
                    }
                }

                return new SuccessResult("Güncelleme işlemi başarılı");

            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<IResult> DeleteAddressAsync(AddressDto addressDto)
        {
            try
            {
                Address address = await _unitOfWork.Addresses.GetAsync(a => a.Id == addressDto.Id);
                if (address == null)
                {
                    return new ErrorResult("Adres bulunamadı", "NotFound");
                }
                await _unitOfWork.Addresses.DeleteAsync(address);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessResult("Adres başarıyla silindi");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }
    }
}
