using Core.Entities.Concrete;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Security.RefreshToken
{
    public class RefreshTokenHelper
    {
        private readonly UserManager<User> _userManager;

        public RefreshTokenHelper(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GenerateAndStoreRefreshTokenAsync(User user)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiration = DateTime.UtcNow.AddHours(1);

            var existingToken = await _userManager.GetAuthenticationTokenAsync(user, "CustomRefreshToken", "RefreshToken");
            if (existingToken != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "CustomRefreshToken", "RefreshToken");
            }

            await _userManager.SetAuthenticationTokenAsync(user, "CustomRefreshToken", "RefreshToken",refreshToken);

            return refreshToken;
        }
    }
}
