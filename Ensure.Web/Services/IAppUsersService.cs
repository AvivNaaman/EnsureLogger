using System;
using System.Threading.Tasks;
using Ensure.Domain.Models;
using Ensure.Web.Data;

namespace Ensure.Web.Services
{
    public interface IAppUsersService
    {
        public Task SetUserTarget(short target, string userName);
        public Task<int> GetUserTarget(string userName);
        public string GenerateBearerToken(AppUser user);
        public Task<ApiUserInfo> GetUserInfo(string userName, string jwtToken);
        public ApiUserInfo GetUserInfo(AppUser u, string jwtToken);
        public Task SendPasswordResetEmail(AppUser u, string resetPasswordUrl);
    }
}
