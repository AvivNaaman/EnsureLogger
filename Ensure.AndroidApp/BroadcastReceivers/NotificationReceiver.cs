using System;
using Android.Content;
using Android.Util;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.BroadcastReceivers
{
    /// <summary>
    /// A Helper BroadcastReceiver to handle delayed notification intents,
    /// coming from NotificationHelper.ScheduleNotification.
    /// </summary>
    [BroadcastReceiver]
    public class EnsureNotificationReceiver : BroadcastReceiver
    {
        DateTime EndTime = DateTime.Today.AddHours(22); // 22:00
        DateTime StartTime = DateTime.Today.AddHours(8); // 8:00

        public override async void OnReceive(Context context, Intent intent)
        {
            Log.Info("EnsureLog", "Notification receiver called!");
            try
            {
                // just call NotificationHelper.Notify(,,)
                var ensureRepository = new EnsureRepository(context);
                var userSvc = new UserService(context);
                if (!userSvc.IsUserLoggedIn)
                {
                    userSvc.LoadUserInfoFromSp(); // Load from SP all the user's info. We may call this after boot!
                }
                var userInfo = await userSvc.RefreshInfo();
                var progress = (await ensureRepository.GetLogs(DateTime.MinValue)).Count;
                Log.Info("EnsureLog", $"Got info from server: {progress}/{userInfo.DailyTarget}");

                if (userInfo.DailyTarget > progress)
                {
                    NotificationHelper.NotifyEnsure(context, progress, userInfo);

                    // schedule next one
                    int left = userInfo.DailyTarget - progress;

                    var allowdHoursTimeSpan = EndTime - StartTime; // total allowd time at a day
                    var notificationsDelay = allowdHoursTimeSpan / (left + 1); // delay between current notification & next one

                    NotificationHelper.ScheduleEnsureCheckNotification(context, DateTime.Now.Add(notificationsDelay));
                }

            }
            catch (Exception e)
            {
                Log.Error("EnsureLog", $"ERROR! Message: `{e.Message}` StackTrace: `{e.StackTrace}`");
            }
        }
    }
}
