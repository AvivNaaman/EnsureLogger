using Ensure.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Domain.Models
{
	public class ApiEnsuresList
	{
		public DateTime CurrentReturnedDate { get; set; }
		public List<EnsureLog> Logs { get; set; }
	}
}
