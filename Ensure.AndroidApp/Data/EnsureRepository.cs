﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Ensure.AndroidApp.Helpers;
using Ensure.Domain;
using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using Ensure.Domain.Models;
using Newtonsoft.Json;
using SQLite;

namespace Ensure.AndroidApp.Data
{
    /// <summary>
    /// A synced respository for ensure logs
    /// </summary>
    public class EnsureRepository : BaseService, IDisposable
    {
        /// <summary>
        /// The SQLite3 database name
        /// </summary>
        const string DBName = "EnsureStore.db";

        /// <summary>
        /// The currently opened SQLite connection
        /// </summary>
        private readonly SQLiteAsyncConnection db;

        /// <summary>
        /// Constructs a new repository.
        /// </summary>
        /// <param name="context"></param>
        public EnsureRepository(Context context) : base(context)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DBName);
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            db = new SQLiteAsyncConnection(path);
            db.CreateTableAsync<InternalEnsureLog>().Wait();
        }

        /// <summary>
        /// Fetchs the logs if online and updates the database or returns the
        /// locally cached logs.
        /// </summary>
        /// <param name="date">The logged date</param>
        /// <returns>A list of logs logged in date</returns>
        public async Task<List<InternalEnsureLog>> GetLogs(DateTime date)
        {
            if (IsInternetConnectionAvailable()) // if there's internet, fetch & update cache:
            {
                string dateQuery = date > DateTime.MinValue ? date.ToString(EnsureConstants.DateTimeUrlFormat) : string.Empty;
                var res = await http.GetAsync($"/api/Ensure/GetLogs?date={dateQuery}");
                if (!res.IsSuccessStatusCode)
                {
                    HandleHttpError(res);
                    return null;
                }
                var ensuresList = JsonConvert.DeserializeObject<ApiEnsuresList>(await res.Content.ReadAsStringAsync());

                // convert all EnsureLogs from server to InternalEnsureLogs for database
                var ensures = ensuresList.Logs.Select(e => new InternalEnsureLog
                {
                    Id = e.Id,
                    EnsureTaste = e.EnsureTaste,
                    UserId = e.UserId,
                    Logged = e.Logged,
                    IsSynced = true
                }).ToList();

                // cache all (delete old that are already there & insert the new ones):
                var day = ensuresList.CurrentReturnedDate.Date;
                await db.ExecuteAsync("DELETE FROM EnsureLogs WHERE Logged < ? AND ? <= Logged;", day.AddDays(1).Ticks.ToString(), day.Ticks.ToString());
                await db.InsertAllAsync(ensures);

                return ensures;
            }
            else // if there is no internet, pull from cache:
            {
                // first in is on top (desc order)
                date = date == DateTime.MinValue ? DateTime.Today : date;
                try
                {
                    // query all between date & day after it
                    var ensures = await db.QueryAsync<InternalEnsureLog>(
                        "SELECT * FROM EnsureLogs WHERE Logged < ? AND ? <= Logged;",
                        date.AddDays(1).Ticks.ToString(), date.Ticks.ToString());
                    return ensures;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Inserts a log to the table async and syncs with server, if network available.
        /// </summary>
        /// <param name="taste">The new log taste</param>
        /// <returns>The newly created log</returns>
        public async Task<InternalEnsureLog> AddLogAsync(EnsureTaste taste)
        {
            InternalEnsureLog log;
            // if net available - push to server
            if (IsInternetConnectionAvailable())
            {
                var res = await http.PostAsync($"/api/Ensure/AddLog?taste={(int)taste}");
                if (!res.IsSuccessStatusCode) // failure
                {
                    HandleHttpError(res);
                    return null;
                }
                else
                {
                    log = JsonConvert.DeserializeObject<InternalEnsureLog>(await res.Content.ReadAsStringAsync());
                    log.IsSynced = true; // becuase got it from server
                }
            }
            // if net not available - generate temporary ID until synced.
            else
            {
                log = new InternalEnsureLog()
                {
                    EnsureTaste = taste,
                    Id = Guid.NewGuid().ToString(),
                    IsSynced = false,
                    UserId = ((EnsureApplication)context.ApplicationContext).UserInfo.Id
                };
            }
            await db.InsertAsync(log);
            return log;
        }

        private void HandleHttpError(HttpResponseMessage message)
        {
            // if unauthorized (=authentication is required), go to login page.
            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Intent i = new Intent(context, typeof(LoginActivity));
                context.StartActivity(i);
            }
        }

        /// <summary>
        /// Pushes all the unsynced ensures to the server
        /// </summary>
        public async Task SyncEnsures()
        {
            // TODO: Add Bulk insertion support on server side!
            if (IsInternetConnectionAvailable())
            {
                // query all unsynced on local db
                var toPush = await db.Table<InternalEnsureLog>()
                    .Where(l => l.IsSynced != true).ToListAsync();

                // post each
                foreach (var log in toPush)
                {
                    // push to server as new
                    var res = await http.PostAsync($"/api/Ensure/AddLog?taste={(int)log.EnsureTaste}");
                    if (!res.IsSuccessStatusCode)
                    {
                        HandleHttpError(res);
                        continue;
                    }
                    var newLog = JsonConvert.DeserializeObject<InternalEnsureLog>(await res.Content.ReadAsStringAsync());
                    // replace old cached with new fetched to avoid duplicates
                    await db.DeleteAsync(log);
                    await db.InsertAsync(newLog);
                }
            }
        }

        /// <summary>
        /// Clears all the local ensures cache
        /// </summary>
        public Task ClearAllCache() => db.Table<InternalEnsureLog>().DeleteAsync();

        public void Dispose()
        {
            db.CloseAsync();
        }
    }
}
