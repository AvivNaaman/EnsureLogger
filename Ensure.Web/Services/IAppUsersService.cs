using System;
using System.Threading.Tasks;
using Ensure.Domain.Models;
using Ensure.Web.Data;

namespace Ensure.Web.Services
{
    public interface IAppUsersService
    {
        /// <summary>
        /// Updates the specified user's target to the specified target
        /// </summary>
        public Task SetUserTarget(int target, string userName);
        /// <summary>
        /// Returns the specified user's current target
        /// </summary>
        /// <returns>The user's target</returns>
        public Task<int> GetUserTarget(string userName);
        /// <summary>
        /// Generates a JWT (api auth) token for the specified user.
        /// </summary>
        /// <returns>The generated token</returns>
        public string GenerateBearerToken(AppUser user);
        /// <summary>
        /// Returns the user info model for the specified user and it's token)
        /// </summary>
        /// <returns>The user info model</returns>
        public Task<ApiUserInfo> GetUserInfo(string userName, string jwtToken);
        /// <summary>
        /// Build a user info model out of the specified user and it's token
        /// </summary>
        /// <returns>The user info model</returns>
        public ApiUserInfo GetUserInfo(AppUser user, string jwtToken);
        /// <summary>
        /// Sends a reset password email to the specified user
        /// </summary>
        /// <param name="resetPasswordUrl">The reset password page url</param>
        public Task SendPasswordResetEmail(AppUser user, string resetPasswordUrl);
        /// <summary>
        /// Returns the user by it's id, AS READ-ONLY.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<AppUser> FindByIdReadonlyAsync(string id);
    }
}
