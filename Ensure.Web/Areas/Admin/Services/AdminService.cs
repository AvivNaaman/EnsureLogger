using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using Ensure.Web.Areas.Admin.Models;
using Ensure.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Ensure.Web.Areas.Admin.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<AdminHomeModel> GetStatsForHome()
        {
            const int daysToQuery = 30;
            var before30days = DateTime.Now.Date.AddDays(-daysToQuery);

            AdminHomeModel toReturn = new();
            toReturn.EnsuresData = new int[35];
            toReturn.UsersData = new int[35];
            toReturn.StartDayFor30 = before30days;

            await QueryAfterAndGroupByDays(_context.Users, u => u.Joined,
                u => u.Joined >= before30days, before30days, toReturn.UsersData);
            await QueryAfterAndGroupByDays(_context.Logs, l => l.Logged,
                l => l.Logged >= before30days, before30days, toReturn.EnsuresData);

            var vals = Enum.GetValues<EnsureTaste>();
            toReturn.TasteData = new int[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                toReturn.TasteData[i] = await _context.Logs
                    .Where(l => l.EnsureTaste == vals[i])
                    .CountAsync();
            }

            toReturn.TotalLoggedEnsures = await _context.Logs.CountAsync();
            toReturn.TotalRegisteredUsers = await _context.Users.CountAsync();

            return toReturn;
        }

        /// <summary>
        /// Queries the number of inserted items to the database after the from date,
        /// and copies the result to the destination array.
        /// </summary>
        public static async Task QueryAfterAndGroupByDays<TEntity>(DbSet<TEntity> toQuery, Func<TEntity, DateTime> dateSelector,
            Expression<Func<TEntity, bool>> selector, DateTime from, int[] destination) where TEntity : class
        {
            // get all entities after date from
            var afterFrom = await toQuery.Where(selector).ToListAsync();
            // group by days after filter
            var groupedByDays = afterFrom.GroupBy(e => dateSelector(e).Date);
            // move to an array
            var countsArr = groupedByDays.ToDictionary(e => e.Key, e => e.Count());
            // copy the result to destination array
            int i = 0;
            for (DateTime d = from.Date; d <= DateTime.Now; d += TimeSpan.FromDays(1), i++)
            {
                if (countsArr.TryGetValue(d, out int r))
                {
                    destination[i] = r;
                }
            }
        }
    }
}
