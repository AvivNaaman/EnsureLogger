using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Models
{
	public class SignUpViewModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string PasswordVertification { get; set; }
		public string Email { get; set; }
		public string DailyTarget { get; set; }
		public short TimeZone { get; set; }
	}
}
