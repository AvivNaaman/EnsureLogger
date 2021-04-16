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

        /// <summary>
        /// Returns all the logged logs in the specified date for a user by it's id
        /// </summary>
        /// <returns>The logs</returns>
        public Task<List<EnsureLog>> GetLogsByDay(string userId, DateTime date);

        /// <summary>
        /// Finds a log by it's id
        /// </summary>
        /// <param name="id">the log id</param>
        /// <returns>The log or null, if not found</returns>
        public Task<EnsureLog> FindByIdAsync(string id);


        /// <summary>
        /// Syncs all the given logs: pushes the new changes to the database and returns all the added logsz
        /// </summary>
        /// <param name="changes">The change models list</param>
        /// <returns>The created logs</returns>
        public Task<ActionResult<List<EnsureLog>>> SyncEnsuresAsync(string userId, List<EnsureSyncModel> logs);
    }
}
