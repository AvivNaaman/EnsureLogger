using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Support.V7.App;
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
    public class EnsureRepository : BaseService
    {
        /// <summary>
        /// The SQLite3 database name
        /// </summary>
        const string DBName = "EnsureStore.db";

        /// <summary>
        /// The currently opened SQLite connection
        /// </summary>
        private SQLiteAsyncConnection db;

        /// <summary>
        /// Constructs a new repository.
        /// </summary>
        /// <param name="context"></param>
        public EnsureRepository(Context context) : base(context)
        {
        }

        private async Task OpenDbConnection()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DBName);
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            db = new SQLiteAsyncConnection(path);
            await db.CreateTableAsync<InternalEnsureLog>();
        }

        private async Task CloseDbConnection()
        {
            await db.CloseAsync();
        }

        /// <summary>
        /// Fetchs the logs if online and updates the database or returns the
        /// locally cached logs.
        /// </summary>
        /// <param name="date">The logged date</param>
        /// <returns>A list of logs logged in date</returns>
        public async Task<List<InternalEnsureLog>> GetLogs(DateTime date, bool forceCache = false)
        {
            List<InternalEnsureLog> ensures;
            await OpenDbConnection();
            if (IsInternetConnectionAvailable() && !forceCache) // if there's internet AND not forcing cache, fetch & update cache:
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
                ensures = ensuresList.Logs.Select(e => new InternalEnsureLog
                {
                    Id = e.Id,
                    EnsureTaste = e.EnsureTaste,
                    UserId = e.UserId,
                    Logged = e.Logged,
                    SyncState = EnsureSyncState.Synced
                }).ToList();

                // cache all (delete old that are already there & insert the new ones):
                var day = ensuresList.CurrentReturnedDate.Date;
                await db.RunInTransactionAsync(t =>
                {
                    t.Execute("DELETE FROM EnsureLogs WHERE Logged < ? AND ? <= Logged;", day.AddDays(1).Ticks.ToString(), day.Ticks.ToString());
                    t.InsertAll(ensures);
                });
                await CloseDbConnection();

            }
            else // if there is no internet, pull from cache:
            {
                // first in is on top (desc order)
                date = date == DateTime.MinValue ? DateTime.Today : date;
                // query all between date & day after it
                ensures = await db.QueryAsync<InternalEnsureLog>(
                    "SELECT * FROM EnsureLogs WHERE Logged < ? AND ? <= Logged;",
                    date.AddDays(1).Ticks.ToString(), date.Ticks.ToString());
            }
            await CloseDbConnection();
            return ensures;
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
                using (var res = await http.PostAsync($"/api/Ensure/AddLog?taste={(int)taste}"))
                {
                    if (!res.IsSuccessStatusCode) // failure
                    {
                        HandleHttpError(res);
                        return null;
                    }
                    else
                    {
                        log = JsonConvert.DeserializeObject<InternalEnsureLog>(await res.Content.ReadAsStringAsync());
                        log.SyncState = EnsureSyncState.Synced; // becuase got it from server
                    }
                }
            }
            // if net not available - generate temporary ID until synced.
            else
            {
                log = new InternalEnsureLog()
                {
                    EnsureTaste = taste,
                    Id = Guid.NewGuid().ToString(),
                    SyncState = EnsureSyncState.ToAdd,
                    UserId = ((EnsureApplication)context.ApplicationContext).UserInfo.Id
                };
            }
            await OpenDbConnection();
            await db.InsertAsync(log);
            await CloseDbConnection();
            return log;
        }

        private void HandleHttpError(HttpResponseMessage message)
        {
            // TODO: Check for better thing to do here?
            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }
            else
            {
                new AlertDialog.Builder(context)
                    .SetMessage($"HTTP Error {message.StatusCode} - {message.ReasonPhrase}")
                    .SetTitle("HTTP Error")
                    .Create().Show();
            }
        }

        public async Task RemoveLogAsync(string logId)
        {
            await OpenDbConnection();
            if (IsInternetConnectionAvailable())
            {
                using (var res = await http.PostAsync($"/api/Ensures/RemoveLog?id={logId}")) // remove from remote
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        HandleHttpError(res);
                        return;
                    }
                    // remove from local
                    await db.Table<InternalEnsureLog>().DeleteAsync(t => t.Id == logId);
                }
            }
            else
            {
                var toRemove = await db.Table<InternalEnsureLog>()
                    .FirstOrDefaultAsync(l => l.Id == logId);
                if (toRemove.SyncState == EnsureSyncState.Synced) // if offline and we have to update cache later
                {
                    // set sync state to "to remove"
                    toRemove.SyncState = EnsureSyncState.ToRemove;
                    await db.UpdateAsync(toRemove);
                }
            }
            await CloseDbConnection();
        }

        /// <summary>
        /// Pushes all the unsynced ensures to the server
        /// </summary>
        public async Task SyncEnsures()
        {
            if (IsInternetConnectionAvailable())
            {
                await OpenDbConnection();
                // query all unsynced on local db
                var toPush = await db.Table<InternalEnsureLog>()
                    .Where(l => l.SyncState != EnsureSyncState.Synced).ToListAsync();

                if (toPush.Count <= 0) return; // nothing to push? skip.

                // Build model for server
                var toPost = new List<EnsureSyncModel>();
                toPush.ForEach(tp =>
                {
                    if (tp.SyncState == EnsureSyncState.ToAdd)
                        toPost.Add(new EnsureSyncModel { ToAdd = true, ToSync = tp });
                    else
                        toPost.Add(new EnsureSyncModel { ToAdd = false, ToSync = tp });
                });

                // post all & hope for good
                var content =
                    new StringContent(JsonConvert.SerializeObject(toPost),
                    System.Text.Encoding.UTF8);

                using (var res = await http.PostAsync("/api/Ensures/SyncLogs", content))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        HandleHttpError(res);
                        return;
                    }

                    // returned from server as created
                    var toInsert = MapServerLogsToInternal(JsonConvert.DeserializeObject<List<EnsureLog>>(await res.Content.ReadAsStringAsync()));

                    // remove all from cache and re-insert
                    await db.RunInTransactionAsync(c =>
                    {
                        c.Table<InternalEnsureLog>().Delete(l => l.SyncState != EnsureSyncState.Synced);
                        c.InsertAll(toInsert);
                    });
                }
                await CloseDbConnection();
            }
        }

        public IEnumerable<InternalEnsureLog> MapServerLogsToInternal(IEnumerable<EnsureLog> input)
        {
            foreach (var item in input)
            {
                yield return new InternalEnsureLog
                {
                    Id = item.Id,
                    EnsureTaste = item.EnsureTaste,
                    SyncState = EnsureSyncState.Synced,
                    Logged = item.Logged,
                    UserId = item.UserId
                };
            }
        }

        /// <summary>
        /// Clears all the local ensures cache
        /// </summary>
        public async Task ClearAllCache()
        {
            await OpenDbConnection();
            await db.Table<InternalEnsureLog>().DeleteAsync();
            await CloseDbConnection();
        }
    }
}
