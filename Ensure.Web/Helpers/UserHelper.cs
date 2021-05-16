using System;
using System.Security.Claims;

namespace Ensure.Web.Helpers
{
    public static class UserExtensions
    {
        /// <summary>
        /// Returns the id of a ClaimsPrincipal user directly.
        /// </summary>
        /// <returns>The user's Id</returns>
        public static string GetId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
