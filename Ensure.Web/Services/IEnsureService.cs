using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Services
{
	public interface IEnsureService
	{
		public Task<EnsureLog> LogAsync(string userName, EnsureTaste taste);
		public Task RemoveLogAsync(EnsureLog log);
		public Task<List<EnsureLog>> GetUserDayLogsAsync(string userName, DateTime date);
		public Task<EnsureLog> FindByIdAsync(string id);
		public Task<int> GetDayCountAsync(string userName, DateTime date);
	}
}
