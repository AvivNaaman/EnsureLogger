using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Domain.Models
{
	public class ApiUserInfo
	{
		public ApiUserInfo()
		{

		}
		public string UserName { get; set; }
		public string Email { get; set; }
		public int DailyTarget { get; set; }
		public string JwtToken { get; set; }
        public string Id { get; set; }
    }
}
