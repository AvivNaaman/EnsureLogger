using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using Ensure.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ensure.Web.Services
{
	public interface IEnsureService
	{
		/// <summary>
        /// Logs a single ensure
        /// </summary>
        /// <param name="userName">The name of logging user</param>
        /// <param name="taste">The taste of the new log</param>
        /// <returns>The created log</returns>
		public Task<EnsureLog> LogAsync(string userName, EnsureTaste taste);

		/// <summary>
        /// Remove a single log
        /// </summary>
        /// <param name="log">The log to remove</param>
		public Task RemoveLogAsync(EnsureLog log);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="date"></param>
        /// <returns></returns>
		public Task<List<EnsureLog>> GetUserDayLogsAsync(string userName, DateTime date);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public Task<EnsureLog> FindByIdAsync(string id);
        [Obsolete]
		public Task<int> GetDayCountAsync(string userName, DateTime date);

        /// <summary>
        /// Adds a list of logs by the specified logs
        /// </summary>
        /// <param name="name">The name of the logging user</param>
        /// <param name="logs">The logs to insert</param>
        /// <returns>The inserted logs</returns>
        public Task<ActionResult<List<EnsureLog>>> SyncEnsuresAsync(string userName, List<EnsureSyncModel> logs);
    }
}
