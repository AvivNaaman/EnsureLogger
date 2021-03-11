using Ensure.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Data
{
	public class AppUser : IdentityUser
	{
		public List<EnsureLog> Logs { get; set; }
		public int DailyTarget { get; set; }
        public DateTime Joined { get; set; }
    }
}
