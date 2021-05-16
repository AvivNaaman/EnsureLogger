using System;
using System.Threading.Tasks;
using Ensure.Domain.Models;
using Ensure.Web.Data;
using Ensure.Web.Models;

namespace Ensure.Web.Services
{
    public interface IAppUsersService
    {
        /// <summary>
        /// Updates the specified user's target to the specified target
        /// </summary>
        public Task<UserResultModel> SetUserTarget(int target, string userName);

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


        public Task<AppUser> FindByNameAsync(string username);

        /// <summary>
        /// Sends a reset password email to the specified user
        /// </summary>
        /// <param name="resetPasswordUrl">The reset password page url</param>
        public Task SendPasswordResetEmail(AppUser user, string resetPasswordUrl);

        public bool CheckPassword(AppUser u, string password);

        /// <summary>
        /// Returns the user by it's id, AS READ-ONLY.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<AppUser> FindByIdReadonlyAsync(string id);

        public bool ValidatePasswordResetToken(AppUser u, string token);

        public string GeneratePasswordResetToken(AppUser u);

        Task<UserResultModel> CreateAsync(AppUser u, string password);

        Task<AppUser> FindByEmailAsync(string email);

        Task<UserResultModel> ResetPasswordAsync(AppUser u, string token, string newPassword);

        public Task DeleteAsync(AppUser u);
    }
}
