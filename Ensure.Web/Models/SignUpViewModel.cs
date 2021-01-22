using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ensure.Web.Models
{
	public class SignUpViewModel
	{
		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password Vertification")]
		[Compare(nameof(Password), ErrorMessage = "Password and vertification must match.")]
		public string PasswordVertification { get; set; }
		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email Address")]
		public string Email { get; set; }
		[Required]
		[Display(Name = "Your Daily Target (you can change later)")]
		public short DailyTarget { get; set; }
		public short TimeZone { get; set; } = 2;
	}
}
