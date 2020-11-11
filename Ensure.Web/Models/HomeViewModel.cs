using Ensure.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Models
{
	public class HomeViewModel
	{
		public List<EnsureLog> Logs { get; set; }
		public DateTime CurrentDate { get; set; }
	}
}
