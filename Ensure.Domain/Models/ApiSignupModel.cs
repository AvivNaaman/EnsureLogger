using System;
namespace Ensure.Domain.Models
{
    public class ApiSignupModel
    {
		public string UserName { get; set; }
		public string Password { get; set; }
		public string PasswordVertification { get; set; }
		public string Email { get; set; }
		public int DailyTarget { get; set; }
	}
}
