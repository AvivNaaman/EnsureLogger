using System;
using System.Threading.Tasks;
using Ensure.Web.Data;

namespace Ensure.Web.Services
{
    public interface IAppUsersService
    {
        public Task SetUserTarget(short target, string userName);
        public Task<int> GetUserTarget(string userName);
        public string GenerateBearerToken(AppUser user);
    }
}
