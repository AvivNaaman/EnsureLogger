﻿using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using Ensure.Domain.Models;
using Ensure.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ensure.Web.Services
{
	public class EnsureService : IEnsureService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly ApplicationDbContext _dbContext;

		public EnsureService(UserManager<AppUser> userManager, ApplicationDbContext dbContext)
		{
			_userManager = userManager;
			_dbContext = dbContext;
		}

		public Task<EnsureLog> FindByIdAsync(string id)
		{
			return _dbContext.Logs.FirstOrDefaultAsync(l => l.Id == id);
		}

		public async Task<List<EnsureLog>> GetUserDayLogsAsync(string userName, DateTime date)
		{
			var u = await _userManager.FindByNameAsync(userName);

			var dayAfterDate = date.AddDays(1);

			var r = await _dbContext.Logs.Where(l => l.Logged >= date && l.Logged < dayAfterDate && l.UserId == u.Id).ToListAsync();

			r.ForEach(l => l.Logged.AddHours(u.TimeZone));
			return r;
		}

		public async Task<int> GetDayCountAsync(string userName, DateTime date)
		{
			var u = await _userManager.FindByNameAsync(userName);

			var dayAfterDate = date.AddDays(1);

			return await _dbContext.Logs.Where(l => l.Logged >= date && l.Logged < dayAfterDate && l.UserId == u.Id).CountAsync();
		}

		public async Task<EnsureLog> LogAsync(string userName, EnsureTaste taste)
		{
			var u = await _userManager.FindByNameAsync(userName);
			var l = new EnsureLog
			{
				UserId = u.Id,
				EnsureTaste = taste
			};
			_dbContext.Logs.Add(l);
			await _dbContext.SaveChangesAsync();
			return l;
		}
		public async Task RemoveLogAsync(EnsureLog log)
		{
			_dbContext.Remove(log);
			await _dbContext.SaveChangesAsync();
		}

        public async Task<ActionResult<List<EnsureLog>>> SyncEnsuresAsync(string userName, List<EnsureSyncModel> logs)
		{
			var u = await _userManager.FindByNameAsync(userName);

			var addedEnsures = new List<EnsureLog>();

			logs.ForEach(l => {
				if (l.ToAdd)
                {
					var newl = new EnsureLog(l.ToSync); // renew log ID
					addedEnsures.Add(newl);
					_dbContext.Logs.Add(newl);
				}
				else
					_dbContext.Logs.Remove(l.ToSync);
			});

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (DbUpdateException)
            {
				// TODO: Do Something?
				throw;
            }
			return addedEnsures;
		}
    }
}
