using Ensure.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Services
{
	public interface ITimeService
	{
		public Task<int> GetUserGmtTimeZoneAsync(string userName);
		public Task<int> GetUserGmtTimeZoneAsync(AppUser user);
	}
}
