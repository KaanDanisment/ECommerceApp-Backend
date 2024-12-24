using Business.Abstract;
using Core.Utilities.Results.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
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

        public AccountController(IAccountService accountManager, IAuthService authManager)
        {
            _accountManager = accountManager;
            _authManager = authManager;
        }


        [HttpGet("getuserinfos")]
        public async Task<IActionResult> GetUserInfos()
        {
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);

            if (accessToken == null && refreshToken == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var user = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!user.Success)
            {
                return Unauthorized(new { Message = user.Message });
            }

            var userInfos = await _accountManager.GetUserInfos(user.Data).ConfigureAwait(false);
            if (userInfos.Success)
            {
                return Ok(userInfos.Data);
            }

            return StatusCode(500, userInfos.Message);
        }

        [HttpGet("getuseraddresses")]
        public async Task<IActionResult> GetUserAddresses()
        {
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);

            if (accessToken == null && refreshToken == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var user = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!user.Success)
            {
                return Unauthorized(new { Message = user.Message });
            }

            var result = await _accountManager.GetUsersAdressesAsync(user.Data).ConfigureAwait(false);
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
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);

            if (accessToken == null && refreshToken == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var user = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!user.Success)
            {
                return Unauthorized(new { Message = user.Message });
            }

            var result = await _accountManager.CreateAddressAsync(addressDto, user.Data).ConfigureAwait(false);
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
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);

            if (accessToken == null && refreshToken == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var user = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!user.Success)
            {
                return Unauthorized(new { Message = user.Message });
            }

            if (userDto.Id != user.Data.Id)
            {
                return Unauthorized(new { Message = "Kullanıcı bilgileri uyuşmuyor. Lütfen oturumunuzu kontrol edin." });
            }

            var result = await _accountManager.UpdateUserAsync(user.Data, userDto).ConfigureAwait(false);
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
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);
            if (accessToken == null && refreshToken == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var user = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!user.Success)
            {
                return Unauthorized(new { Message = user.Message });
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
