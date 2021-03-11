using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Ensure.Web.Helpers
{
    public static class UserExtensions
    {
        public static string GetId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
