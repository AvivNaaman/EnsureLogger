using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Ensure.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace Ensure.Web.Services
{
    public class AppUsersService : IAppUsersService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration config;

        public AppUsersService(IConfiguration config, UserManager<AppUser> userManager)
        {
            this.config = config;
            _userManager = userManager;
        }

        public string GenerateBearerToken(AppUser user)
        {

            var key = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]);
            var tokenDescriptor = new JwtSecurityToken(null, null, claims: new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                },
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature));
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<int> GetUserTarget(string userName) => (await _userManager.FindByNameAsync(userName)).DailyTarget;

        public async Task SetUserTarget(short target, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);
            u.DailyTarget = target;
            await _userManager.UpdateAsync(u);
        }
    }
}
