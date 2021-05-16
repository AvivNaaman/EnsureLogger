using Ensure.Domain;
using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using Ensure.Domain.Models;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Diagnostics;
using System.Threading.Tasks;
using Ensure.Web.Helpers;

namespace Ensure.Web.Controllers
{
	[Route("api/Ensure")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // use JWT Bearer for API
	public class EnsureApiController : ControllerBase
	{
		private readonly IEnsureService _ensureService;

		public EnsureApiController(IEnsureService ensureService)
		{
			_ensureService = ensureService;
		}

		/// <summary>
        /// Logs a new ensure with the specified taste for the current user.
        /// </summary>
        /// <param name="taste">The ensure taste (as query string)</param>
        /// <returns>The newly created log</returns>
		[Route("AddLog")]
		[HttpPost]
		public async Task<ActionResult<EnsureLog>> AddLog(EnsureTaste taste)
		{
			return await _ensureService.LogAsync(UserExtensions.GetId(User), taste);
		}

		/// <summary>
        /// Syncs all the client's ensures with the database
        /// </summary>
        /// <param name="toSync">The ensures sync model</param>
        /// <returns>The CREATED ensures</returns>
		[Route("SyncLogs")]
		[HttpPost]
		public async Task<ActionResult<List<EnsureLog>>> SyncLogs(List<EnsureSyncModel> toSync)
        {
			return await _ensureService.SyncEnsuresAsync(UserExtensions.GetId(User), toSync);
        }

		/// <summary>
        /// Removes a specific ensure log
        /// </summary>
        /// <param name="id">The id of the log to remove</param>
		[Route("RemoveLog")]
		[HttpPost]
		public async Task<ActionResult> RemoveLog(string id)
		{
			var l = await _ensureService.FindByIdAsync(id);
			if (l == null || l.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
			{
				await _ensureService.RemoveLogAsync(l);
				return Ok();
			}
			else return BadRequest();
		}

		/// <summary>
        /// Returns the logs of a specific date
        /// </summary>
        /// <param name="date">The date to look for logs on</param>
        /// <returns>The logs</returns>
		[Route("GetLogs")]
		[HttpGet]
		public async Task<ActionResult<ApiEnsuresList>> GetLogs(string date)
		{
			// user's time
			DateTime d = DateTime.UtcNow;
			try
			{
				d = DateTime.ParseExact(date, EnsureConstants.DateTimeUrlFormat, CultureInfo.InvariantCulture);
			}
			catch { }
			var el = await _ensureService.GetLogsByDay(UserExtensions.GetId(User), d.Date);
			return new ApiEnsuresList { CurrentReturnedDate = d, Logs = el };
		}
	}
}
