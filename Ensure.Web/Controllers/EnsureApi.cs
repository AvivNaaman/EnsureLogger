﻿using Ensure.Domain;
using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
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
using System.Threading.Tasks;

namespace Ensure.Web.Controllers
{
	[Route("api/Ensure")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // use JWT Bearer for API
	public class EnsureApi : ControllerBase
	{
		private readonly IEnsureService _ensureService;
		private readonly ITimeService _timeService;

		public EnsureApi(IEnsureService ensureService, ITimeService timeService)
		{
			_ensureService = ensureService;
			_timeService = timeService;
		}

		[Route("[action]")]
		[HttpPost]
		public async Task<ActionResult<EnsureLog>> AddLog(EnsureTaste taste)
		{
			return await _ensureService.LogAsync(User.Identity.Name, taste);
		}



		[Route("[action]")]
		public async Task<ActionResult> RemoveLog(string id)
		{
			var l = await _ensureService.FindByIdAsync(id);
			if (l.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
			{
				await _ensureService.RemoveLogAsync(l);
				return Ok();
			}
			else return BadRequest();
		}

		[Route("[action]")]
		public async Task<ActionResult<List<EnsureLog>>> GetLogs(string date)
		{

			int userTimeZone = await _timeService.GetUserGmtTimeZoneAsync(User.Identity.Name);
			// user's time
			DateTime d = DateTime.UtcNow.Add(TimeSpan.FromHours(userTimeZone));
			try
			{
				d = DateTime.ParseExact(date, EnsureConstants.DateTimeUrlFormat, CultureInfo.InvariantCulture);
			}
			catch { }

			return await _ensureService.GetUserDayLogsAsync(User.Identity.Name,
					d.Date.Subtract(TimeSpan.FromHours(userTimeZone)));
		}
	}
}
