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
    public class AuthController : Controller
    {
        private readonly IAuthService _authManager;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authManager, UserManager<User> userManager)
        {
            _authManager = authManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var login = await _authManager.LoginAsync(userLoginDto).ConfigureAwait(false);
            if (!login.Success)
            {
                if (login is ErrorDataResult<User> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { errorDataResult.Message });
                    }
                    else if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                    else if (errorDataResult.ErrorType == "NotFound")
                    {
                        return NotFound(new { errorDataResult.Message });
                    }
                }
            }
            var jwtToken = await _authManager.CreateJwtTokenAsync(login.Data).ConfigureAwait(false);
            if (!jwtToken.Success)
            {
                if (jwtToken is ErrorDataResult<AccessToken> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
                    }
                }
            }
            var refreshToken = await _authManager.CreateRefreshTokenAsync(login.Data).ConfigureAwait(false);
            if (!refreshToken.Success)
            {
                if (refreshToken is ErrorDataResult<AccessToken> errorDataResult)
                {
                    if (errorDataResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorDataResult.Message);
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

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return NotFound(new { Message = "Refresh token not found" });

            var tokenIsValid = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            if (!tokenIsValid.Success)
            {
                return NotFound(new { Message = tokenIsValid.Message });
            }

            User user = tokenIsValid.Data;
            var newJwtToken = await _authManager.CreateJwtTokenAsync(user).ConfigureAwait(false);
            var newRefreshToken = await _authManager.CreateRefreshTokenAsync(user).ConfigureAwait(false);

            if (!newJwtToken.Success || !newRefreshToken.Success)
            {
                return NotFound(new { Message = "Tokens can not created" });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("AccessToken", newJwtToken.Data.Token, cookieOptions);
            Response.Cookies.Append("RefreshToken", newRefreshToken.Data, cookieOptions);

            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var userExist = await _authManager.UserExist(userRegisterDto.Email).ConfigureAwait(false);
            if (!userExist.Success)
            {
                return BadRequest(new { Message = userExist.Message });
            }

            var register = await _authManager.RegisterAsync(userRegisterDto).ConfigureAwait(false);
            if (!register.Success)
            {
                if (register is ErrorResult errorResult)
                {
                    if (errorResult.ErrorType == "BadRequest")
                    {
                        return BadRequest(new { Message =  errorResult.Message });
                    }
                    else if (errorResult.ErrorType == "SystemError")
                    {
                        return StatusCode(500, errorResult.Message);
                    }
                }
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
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

            if (user != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user.Data, "CustomRefreshToken", "RefreshToken").ConfigureAwait(false);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            Response.Cookies.Delete("AccessToken", cookieOptions);
            Response.Cookies.Delete("RefreshToken", cookieOptions);

            return Ok();
        }

        [HttpGet("isAuthenticated")]
        public async Task<IActionResult> UserAuthenticationControl()
        {
            HttpContext.Request.Cookies.TryGetValue("AccessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);

            if (accessToken != null && refreshToken != null)
            {
                var refreshTokenIsValid = await _authManager.ValidateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
                if (refreshTokenIsValid.Success)
                {
                    return Ok(true);
                }
                return Ok(false);
            }

            return Ok(false);
        }
    }
}
