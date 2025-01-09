using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountManager;
        private readonly IAuthService _authManager;
        private readonly UserManager<User> _userManager;

        public AccountController(IAccountService accountManager, IAuthService authManager, UserManager<User> userManager)
        {
            _accountManager = accountManager;
            _authManager = authManager;
            _userManager = userManager;
        }


        [HttpGet("getuserinfos")]
        public async Task<IActionResult> GetUserInfos()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var userInfos = await _accountManager.GetUserInfos(user).ConfigureAwait(false);
            if (userInfos.Success)
            {
                return Ok(userInfos.Data);
            }

            return StatusCode(500, userInfos.Message);
        }

        [HttpGet("getuseraddresses")]
        public async Task<IActionResult> GetUserAddresses()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var result = await _accountManager.GetUsersAdressesAsync(user).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorDataResult<AddressDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
                return Ok(new { Data = result.Data, Message = result.Message });
            }
            return Ok(result.Data);
        }

        [HttpPost("createaddress")]
        public async Task<IActionResult> CreateAddress(AddressDto addressDto)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var result = await _accountManager.CreateAddressAsync(addressDto, user).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, result.Message);
                    }
                }
            }
            return Ok();
        }

        [HttpPut("updateuser")]
        public async Task<IActionResult> UpdateUser(UserDto userDto)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            if (userDto.Email != user.Email)
            {
                return Unauthorized(new { Message = "Kullanıcı bilgileri uyuşmuyor. Lütfen oturumunuzu kontrol edin." });
            }

            var result = await _accountManager.UpdateUserAsync(user, userDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message = errorResult.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                }
            }
            return Ok();
        }

        [HttpDelete("deleteaddress")]
        public async Task<IActionResult> DeleteAddress(AddressDto addressDto)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var result = await _accountManager.DeleteAddressAsync(addressDto).ConfigureAwait(false);
            if (!result.Success)
            {
                if (result is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                    else if(errorResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { Message = errorResult.Message });
                    }
                }
            }
            return Ok();
        }
    }
}
