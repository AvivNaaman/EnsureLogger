using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ensure.Web.Data;
using Ensure.Web.Options;
using Ensure.Web.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ensure.Web.Services
{
    public class SigninService : ISigninService
    {
        private readonly HttpContext _httpContext;
        private readonly JwtOptions _jwtOptions;
        private readonly CookieAuthOptions _cookieOptions;
        private readonly IAppUsersService _appUsersService;

        public SigninService(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor contextAccessor,
            IOptions<CookieAuthOptions> cookieOptions, IAppUsersService appUsersService)
        {
            _httpContext = contextAccessor.HttpContext;
            _jwtOptions = jwtOptions.Value;
            _cookieOptions = cookieOptions.Value;
            _appUsersService = appUsersService;
        }

        public bool SessionPasswordLogin(AppUser u, string password)
        {
            if (!_appUsersService.CheckPassword(u, password)) return false;

            return SessionLogin(u);
        }

        public bool SessionLogin(AppUser u)
        {

            var model = new Dictionary<string, string>
            {
                { ClaimTypes.NameIdentifier, u.Id },
                { ClaimTypes.Name, u.UserName },
                { ClaimTypes.Email, u.Email }
            };

            var json = JsonSerializer.Serialize(model);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var signature = new HMACSHA512(_cookieOptions.KeyBytes).ComputeHash(jsonBytes);

            var cookie = $"{Convert.ToBase64String(jsonBytes)}.{Convert.ToBase64String(signature)}";

            _httpContext.Response.Cookies.Delete(SessionAuthConstants.DefaultCookieName);
            _httpContext.Response.Cookies.Append(SessionAuthConstants.DefaultCookieName, cookie);

            return true;
        }

        public void SessionLogout()
        {
            _httpContext.Response.Cookies.Delete(SessionAuthConstants.DefaultCookieName);
        }


        public string GenerateApiLoginToken(AppUser user)
        {
            var tokenDescriptor = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims: new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                },
            expires: DateTime.UtcNow.AddDays(_jwtOptions.DaysToExpire),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                                                        SecurityAlgorithms.HmacSha256Signature));
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
