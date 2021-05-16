using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Ensure.Web.Data;
using System.Linq;
using Ensure.Domain.Models;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ensure.Web.Options;
using Ensure.Web.Models;
using System.Text.Json;
using System.Security.Cryptography;
using Ensure.Domain.Helpers;

namespace Ensure.Web.Services
{
    public class AppUsersService : IAppUsersService
    {
        private readonly SendGridOptions _sendGridOptions;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISender _emailSender;
        private readonly ITemplateRenderer _emailTemplateRenderer;

        public AppUsersService(ApplicationDbContext dbContext, IOptions<SendGridOptions> sendGridOptions,
            ISender emailSender, ITemplateRenderer emailTemplateRenderer)
        {
            _sendGridOptions = sendGridOptions.Value;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
        }

        #region ApiGetInfo
        public async Task<ApiUserInfo> GetUserInfo(string userId, string jwtToken)
        {
            var u = await FindByIdReadonlyAsync(userId);
            if (u is null) return null;
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
        #endregion

        #region PasswordReset 

        public async Task SendPasswordResetEmail(AppUser u, string resetPasswordUrl)
        {
            // Razor template for the HTML email
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
            // generate all reuired data
            var token = GeneratePasswordResetToken(u);
            var encToken = System.Web.HttpUtility.UrlEncode(token);
            var encEmail = System.Web.HttpUtility.UrlEncode(u.Email);
            // build email and send it
            var result = await new Email(_emailTemplateRenderer, _emailSender)
                .To(u.Email, u.UserName)
                .SetFrom(_sendGridOptions.FromAddress, "Ensure Logger")
                .Subject(subj)
                .UsingTemplate(template, new
                {
                    u.UserName,
                    u.Email,
                    ResetUrl = resetPasswordUrl + "?token=" + encToken + "&email=" + encEmail
                }).SendAsync();

            if (!result.Successful)
                throw new Exception(result.ErrorMessages.First());

        }

        public string GeneratePasswordResetToken(AppUser u)
        {
            // Perhaps save creation time in db?
            // Create model
            PasswordResetTokenModel m = new()
            {
                Email = u.Email
            };
            // Encode to json
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(m);
            var b64Json = Convert.ToBase64String(jsonBytes);
            // calculate hmacsha256 signature
            var signature = new HMACSHA512(u.SecurityKey)
                .ComputeHash(jsonBytes);
            // base 64 both, seperate with '.'
            return $"{b64Json}.{Convert.ToBase64String(signature)}";
        }

        public bool ValidatePasswordResetToken(AppUser u, string token)
        {
            if (u is null) return false;

            // desirialize data
            var s = token.Split('.');
            var (b64json, signature) = (s[0], s[1]);
            var jsonBytes = Convert.FromBase64String(b64json);
            var j = JsonSerializer.Deserialize<PasswordResetTokenModel>(jsonBytes);
            // validate all props of desirialized objects
            if (j is null || j.Purpose != PasswordResetTokenModel.ResetPasswordPurpose
                    || j.Email is null or "" || j.Produced == DateTime.MinValue) return false;

            // validate signature
            if (Convert.ToBase64String(
                    new HMACSHA512(u.SecurityKey)
                    .ComputeHash(jsonBytes)
                ) != signature) return false;

            return true;
        }

        public async Task<UserResultModel> ResetPasswordAsync(AppUser u, string token, string newPassword)
        {
            var res = new UserResultModel();
            if (!ValidatePasswordResetToken(u, token))
            {
                res.Errors.Add("Invalid token.");
                return res;
            }

            if (!UserDataValidator.ValidatePassword(newPassword))
            {
                res.Errors.Add("Password does not meet minimum requirments.");
                return res;
            }

            var newPwdHash = HashUserPassword(u, newPassword);
            u.PasswordHash = newPwdHash;

            _dbContext.Update(u);
            await _dbContext.SaveChangesAsync();

            return res;
        }
        #endregion

        #region Passwords
        public bool CheckPassword(AppUser u, string password)
        {
            if (u is null) return false;
            // validate that password hash in db is the same is hash of password param with same security key.
            return u.PasswordHash.SequenceEqual(HashUserPassword(u,password));
        }

        public byte[] HashUserPassword(AppUser u, string password)
        {
            if (u is null) return null;
            return new HMACSHA512(u.SecurityKey).ComputeHash(Encoding.UTF8.GetBytes(password));
        }
        #endregion

        #region Finds

        public Task<AppUser> FindByEmailAsync(string email) => _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

        public Task<AppUser> FindByNameAsync(string username) => _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);

        public Task<AppUser> FindByIdReadonlyAsync(string id)
            => _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

        #endregion

        #region UpdateEditCreate
        public async Task<UserResultModel> CreateAsync(AppUser u, string password)
        {
            var res = new UserResultModel();

            if (!UserDataValidator.ValidateEmail(u.Email))
            {
                res.Errors.Add("Invalid email address.");
            }
            if (!UserDataValidator.ValidateUserName(u.UserName))
            {
                res.Errors.Add("Invalid user name.");
            }
            if (!UserDataValidator.ValidateTarget(u.DailyTarget))
            {
                res.Errors.Add("Target has to be at least 0.");
            }
            if (!UserDataValidator.ValidatePassword(password))
            {
                res.Errors.Add("Password does not meet minimum requirments.");
            }

            if (!res.Succeeded) return res; // stop and return all errors.

            u.PasswordHash = HashUserPassword(u, password);
            _dbContext.Users.Add(u);
            await _dbContext.SaveChangesAsync();
            return res;
        }

        public async Task<UserResultModel> SetUserTarget(int target, string userName)
        {
            var res = new UserResultModel();

            if (!UserDataValidator.ValidateTarget(target))
            {
                res.Errors.Add("Target has to be at least 0.");
                return res;
            }

            var u = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            u.DailyTarget = target;
            _dbContext.Update(u);
            await _dbContext.SaveChangesAsync();
            return res;
        }

        public async Task DeleteAsync(AppUser u)
        {
            _dbContext.Users.Remove(u);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
