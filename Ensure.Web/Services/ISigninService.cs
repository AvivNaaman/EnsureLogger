using Ensure.Web.Data;

namespace Ensure.Web.Services
{
    public interface ISigninService
    {
        /// <summary>
        /// Generates a JWT (api auth) token for the specified user.
        /// </summary>
        /// <returns>The generated token</returns>
        public string GenerateApiLoginToken(AppUser user);
        public bool SessionPasswordLogin(AppUser u, string password);
        public void SessionLogout();
        bool SessionLogin(AppUser u);
    }
}