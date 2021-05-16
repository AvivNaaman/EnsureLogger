﻿using System;
using Android.Content;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Services;

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
            LogHelper.Info("Notification receiver called!");
            try
            {
                // just call NotificationHelper.Notify(,,)
                var ensureRepository = new EnsureRepository(context);
                var userSvc = new UserService(context);
                if (!userSvc.IsUserLoggedIn)
                {
                    userSvc.LoadUserInfoFromSp(); // Load from SP all the user's info. We may call this after boot!
                }
                // pull saved info
                var userInfo = await userSvc.RefreshInfo();
                var progress = (await ensureRepository.GetLogs(DateTime.MinValue)).Count;

                // error has occured during information updating - stop.
                if (userInfo is null || progress < 0)
                {
                    LogHelper.Error("An error has occured while refreshing user info and log count in notification receiver.");
                    return;
                }
                LogHelper.Info($"Got info from server: {progress}/{userInfo.DailyTarget}");

                if (userInfo.DailyTarget > progress)
                {
                    var ns = new NotificationsService(context);
                    // notify user to drink
                    ns.NotifyEnsure(progress, userInfo);

                    // schedule next one
                    int left = userInfo.DailyTarget - progress;

                    var allowdHoursTimeSpan = EndTime - StartTime; // total allowd time at a day
                    var notificationsDelay = allowdHoursTimeSpan / (left + 1); // delay between current notification & next one

                    // schedule next check
                    ns.ScheduleEnsureCheckNotification(DateTime.Now.Add(notificationsDelay));
                }

            }
            catch (Exception e)
            {
                LogHelper.Error($"ERROR! Message: `{e.Message}` StackTrace: `{e.StackTrace}`");
            }
        }
    }
}