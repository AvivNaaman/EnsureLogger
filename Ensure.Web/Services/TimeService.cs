using Ensure.Web.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Services
{
	public class TimeService : ITimeService
	{
        private readonly UserManager<AppUser> _userManager;
		public TimeService(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<int> GetUserGmtTimeZoneAsync(string userName)
		{
			return (await _userManager.FindByNameAsync(userName)).TimeZone;
		}

		public Task<int> GetUserGmtTimeZoneAsync(AppUser user)
		{
			return Task.FromResult<int>(user.TimeZone);
		}
	}
}
