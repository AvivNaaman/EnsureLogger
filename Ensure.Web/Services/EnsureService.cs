using Ensure.Domain.Entities;
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
		private readonly ApplicationDbContext _dbContext;

		public EnsureService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public Task<EnsureLog> FindByIdAsync(string id)
		{
			return _dbContext.Logs.FirstOrDefaultAsync(l => l.Id == id);
		}

		public async Task<List<EnsureLog>> GetLogsByDay(string userId, DateTime date)
		{
			var dayAfterDate = date.AddDays(1);

			var r = await _dbContext.Logs.Where(l => l.Logged >= date && // in date range
												l.Logged < dayAfterDate && // and for the user
												l.UserId == userId).ToListAsync();
			return r;
		}

		public async Task<EnsureLog> LogAsync(string userId, EnsureTaste taste)
		{
			var l = new EnsureLog
			{
				UserId = userId,
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

        public async Task<ActionResult<List<EnsureLog>>> SyncEnsuresAsync(string userId, List<EnsureSyncModel> logs)
		{

			var addedEnsures = new List<EnsureLog>();

			logs.ForEach(l => {
				if (l.ToAdd)
                {
					var newl = new EnsureLog(l.ToSync); // renew log ID
					newl.UserId = userId;
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
