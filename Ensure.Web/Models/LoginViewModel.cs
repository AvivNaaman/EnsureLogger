using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Models
{
	public class LoginViewModel
	{
		[Required]
		[MinLength(4)]
		[MaxLength(20)]
		public string UserName { get; set; }
		[Required]
		[MinLength(6)]
		[MaxLength(20)]
		public string Password { get; set; }
	}
}
