using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results.Concrete;
using Core.Utilities.Security.jwt;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminManager;
        private readonly UserManager<User> _userManager;

        public AdminController(IAdminService adminManager, UserManager<User> userManager)
        {
            _adminManager = adminManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto adminLoginDto)
        {
            var login = await _adminManager.AdminLoginAsync(adminLoginDto).ConfigureAwait(false);
            if (!login.Success)
            {
                if (login is ErrorDataResult<User> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { errorDataResult.Message });
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { errorDataResult.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, new { errorDataResult.Message });
                    }
                }
            }
            var jwtToken = await _adminManager.CreateJwtTokenAsync(login.Data).ConfigureAwait(false);
            if (!jwtToken.Success)
            {
                if (jwtToken is ErrorDataResult<AccessToken> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, new { errorDataResult.Message });
                    }
                }
            }
            var refreshToken = await _adminManager.CreateRefreshTokenAsync(login.Data).ConfigureAwait(false);
            if (!refreshToken.Success)
            {
                if (refreshToken is ErrorDataResult<string> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, new { errorDataResult.Message });
                    }
                }
            }
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("AccessToken", jwtToken.Data.Token, cookieOptions);
            Response.Cookies.Append("RefreshToken", refreshToken.Data, cookieOptions);
            return Ok();
        }

        [HttpPost("createAdmin")]
        public async Task<IActionResult> Register(AdminRegisterDto adminRegisterDto)
        {
            var userExist = await _adminManager.UserExist(adminRegisterDto.Email).ConfigureAwait(false);
            if(!userExist.Success)
            {
                return BadRequest(new { userExist.Message });
            }
            var register = await _adminManager.AdminRegisterAsync(adminRegisterDto).ConfigureAwait(false);
            if (!register.Success)
            {
                if (register is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { errorResult.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, new { errorResult.Message });
                    }
                }
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("getAdminInfos")]
        public async Task<IActionResult> GetAdminInfos()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }
            var adminInfos = await _adminManager.GetAdminInfos(user).ConfigureAwait(false);
            if (!adminInfos.Success)
            {
                if (adminInfos is ErrorDataResult<AdminDto> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { errorDataResult.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, new { errorDataResult.Message });
                    }
                }
            }
            return Ok(adminInfos.Data);
        }
    }
}
