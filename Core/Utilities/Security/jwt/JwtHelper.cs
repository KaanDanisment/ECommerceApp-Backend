using Core.Entities.Concrete;
using Core.Utilities.Security.Encryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Security.jwt
{
    public class JwtHelper : ITokenHelper
    {
        public IConfiguration Configuration { get; }
        private readonly TokenOptions _tokenOptions;
        private readonly DateTime _accessTokenExpiration;
        private readonly UserManager<User> _userManager;

        public JwtHelper(IConfiguration configuration, UserManager<User> userManager)
        {
            Configuration = configuration;
            _tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            _userManager = userManager;
        }

        public async Task<AccessToken> CreateTokenAsync(User user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,user.FirstName),
                new Claim(ClaimTypes.Surname,user.LastName),
            };

            var roles = await _userManager.GetRolesAsync(user);
            userClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialHelper.CreateSigningCredentials(securityKey);
            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: userClaims,
                signingCredentials:signingCredentials
                );
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwt);

            return new AccessToken()
            {
                Token = token,
                Expiration = _accessTokenExpiration,
            };
        }
    }
}
