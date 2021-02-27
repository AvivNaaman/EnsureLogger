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
using Ensure.Domain.Models;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core;

namespace Ensure.Web.Services
{
    public class AppUsersService : IAppUsersService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISender emailSender;
        private readonly ITemplateRenderer emailTemplateRenderer;
        private readonly IConfiguration config;

        public AppUsersService(IConfiguration config, UserManager<AppUser> userManager,
            ISender emailSender, ITemplateRenderer emailTemplateRenderer)
        {
            this.config = config;
            _userManager = userManager;
            this.emailSender = emailSender;
            this.emailTemplateRenderer = emailTemplateRenderer;
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

        public async Task<ApiUserInfo> GetUserInfo(string userName, string jwtToken)
        {
            var u = await _userManager.FindByNameAsync(userName);
            return GetUserInfo(u, jwtToken);
        }

        public ApiUserInfo GetUserInfo(AppUser u, string jwtToken)
        {
            return new ApiUserInfo()
            {
                DailyTarget = u.DailyTarget,
                Email = u.Email,
                Id = u.Id,
                UserName = u.UserName,
                JwtToken = jwtToken
            };
        }

        public async Task<int> GetUserTarget(string userName) => (await _userManager.FindByNameAsync(userName)).DailyTarget;

        public async Task SendPasswordResetEmail(AppUser u, string resetPasswordUrl)
        {
            const string template = @"
<html>
<head>
<title>Password Reset Email</title>
</head>
<body>
Hi, @Model.UserName, click <a href=""@Model.ResetUrl"">here</a> to begin your password reset.
</body>
</html>
";
            const string subj = "Password Reset Email";
            var token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var encToken = System.Web.HttpUtility.UrlEncode(token);
            var encEmail = System.Web.HttpUtility.UrlEncode(u.Email);
            var result = await new Email(emailTemplateRenderer, emailSender)
                .To(u.Email, u.UserName)
                .SetFrom(config["SendGrid:FromAddress"], config["SendGrid:FromName"])
                .Subject(subj)
                .UsingTemplate(template, new
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    ResetUrl = resetPasswordUrl + "?token=" + encToken + "&email=" + encEmail
                }).SendAsync();

            if (!result.Successful)
                throw new Exception(result.ErrorMessages.First());
            
        }

        public async Task SetUserTarget(short target, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);
            u.DailyTarget = target;
            await _userManager.UpdateAsync(u);
        }
    }
}
