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
        /// <param name="userId">The id of logging user</param>
        /// <param name="taste">The taste of the new log</param>
        /// <returns>The created log</returns>
		public Task<EnsureLog> LogAsync(string userId, EnsureTaste taste);

		/// <summary>
        /// Remove a single log
        /// </summary>
        /// <param name="log">The log to remove</param>
		public Task RemoveLogAsync(EnsureLog log);

		public Task<List<EnsureLog>> GetLogsByDay(string userId, DateTime date);

		public Task<EnsureLog> FindByIdAsync(string id);

        /// <summary>
        /// Adds a list of logs by the specified logs
        /// </summary>
        /// <param name="name">The id of the syncing user</param>
        /// <param name="logs">The logs to insert</param>
        /// <returns>The inserted logs</returns>
        public Task<ActionResult<List<EnsureLog>>> SyncEnsuresAsync(string userId, List<EnsureSyncModel> logs);
    }
}
