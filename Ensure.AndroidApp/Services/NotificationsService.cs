using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Ensure.AndroidApp.BroadcastReceivers;
using Ensure.Domain.Models;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Data;
using System.Threading.Tasks;

namespace Ensure.AndroidApp.Services
{
    /// <summary>
    /// A service to manage the notification scheduling and pushing.
    /// </summary>
    public class NotificationsService
    {
        /// <summary>
        /// A constant notification id (so it can be removed later)
        /// </summary>
        const int NotificationId = 1;

        private readonly Context context;

        /// <summary>
        /// Constucts a new notifications service
        /// </summary>
        public NotificationsService(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// Schedules a check for the current drinking status
        /// </summary>
        /// <param name="schedule">The next date to check on</param>
        public void ScheduleEnsureCheckNotification(DateTime schedule, bool withoutNotification)
        {
            var alarm = context.GetSystemService<AlarmManager>(Context.AlarmService);
            long scheduleAsMillisecs = (long)schedule.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            var intent = new Intent(context, typeof(EnsureNotificationReceiver));
            intent.PutExtra("withoutNotification", withoutNotification);

            var pIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);

            // fire scheduling
            alarm.SetExactAndAllowWhileIdle(AlarmType.Rtc, scheduleAsMillisecs, pIntent);
        }

        /// <summary>
        /// Cancells all the scheduled notification checks
        /// </summary>
        public void CancelAllScheduled()
        {
            var alarm = context.GetSystemService<AlarmManager>(Context.AlarmService);

            var intent = new Intent(context, typeof(EnsureNotificationReceiver));

            intent.PutExtra("withoutNotification", true);
            var pi = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);
            alarm.Cancel(pi);

            intent.PutExtra("withoutNotification", false);
            pi = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);
            alarm.Cancel(pi);
        }

        /// <summary>
        /// Handles the notification receiver broadcasts
        /// </summary>
        public async Task HandleNotificationBroadcastAsync(Intent intent)
        {
            // Stage 1: update information from server
            var ensureRepository = new EnsureRepository(context);
            var userSvc = new UserService(context);
            if (!userSvc.IsUserLoggedIn)
            {
                userSvc.LoadUserInfoFromSp(); // Load from SP all the user's info. We may call this after boot!
            }
            var userInfo = await userSvc.RefreshInfo();
            var progress = (await ensureRepository.GetLogs(DateTime.MinValue)).Count;
            if (userInfo is null || progress < 0) // error has occured during information updating - stop.
            {
                LogHelper.Error("An error has occured while refreshing user info and log count in notification handler.");
                return;
            }
            LogHelper.Info($"Notification Service Got info from server: {progress}/{userInfo.DailyTarget}");

            if (!context.IsAppForeground() && !intent.GetBooleanExtra("withoutNotification", false)) 
                // only if user is NOT using the app, and there was no request (int the intent) to prevent showing notifications
            {
                // Stage 2: Notify user to drink if should (Target > Progress)
                if (userInfo.DailyTarget > progress)
                {
                    NotifyEnsure(progress, userInfo);
                }
            }

            // Stage 3: schedule next check
            DateTime nextNotificationSchedule = GetNextSchedule(progress, userInfo);
            ScheduleEnsureCheckNotification(nextNotificationSchedule, false);
        }

        /// <summary>
        /// Returns the next time where a check and notification should get done.
        /// </summary>
        /// <param name="progress">The current progress of the user today</param>
        /// <param name="userInfo">The information of the user</param>
        /// <returns>When should next check get triggered</returns>
        private DateTime GetNextSchedule(int progress, ApiUserInfo userInfo)
        {
            DateTime toReturn = DateTime.Today.AddDays(1).AddHours(8);

            const int StartTime = 9;
            const int EndTime = 21;

            int drinksLeft = userInfo.DailyTarget - progress;
            var timeUntilEnd = DateTime.Today.AddHours(EndTime) - DateTime.Now;

            if (drinksLeft > 0 && // if the user should drink more today
               DateTime.Now.Hour >= StartTime &&
               timeUntilEnd > TimeSpan.Zero) // and in range of notifications
            {
                var timeSpanUntilNext = timeUntilEnd / drinksLeft;
                var timeOfNext = DateTime.Now.Add(timeSpanUntilNext);

                toReturn = timeOfNext;
            }

            return toReturn;
        }

        /// <summary>
        /// Notifies the user to drink
        /// </summary>
        /// <param name="progress">Current progress</param>
        /// <param name="info">Current user's info</param>
        public void NotifyEnsure(int progress, ApiUserInfo info)
        {
            // get all relevant constants from xml
            var notifications = context.GetSystemService<NotificationManager>(Context.NotificationService);
            var channelId = context.GetString(Resource.String.EnsureNotificationChannelId);
            var channelName = context.GetString(Resource.String.EnsureNotificationChannelName);
            var channelDesc = context.GetString(Resource.String.EnsureNotificationChannelDesc);

            string title = $"{info.UserName}, Don't forget to drink!";
            string txt = $"Currently { progress } out of { info.DailyTarget }";


            Notification.Builder notificationBuilder;
            if (BuildVersionCodes.O <= Build.VERSION.SdkInt) // Oreo+ version
            {
                // Create channel for Oreo+
                notifications.CreateNotificationChannel(
                    new NotificationChannel(channelId, channelName, NotificationImportance.High)
                    {
                        Description = channelDesc
                    });
                // create notification
                notificationBuilder = new Notification.Builder(context, channelId);
            }
            else // legacy notification - no channel!
            {
                notificationBuilder = new Notification.Builder(context);
            }

            // finish building notification
            var notification = notificationBuilder
                    .SetContentTitle(title)
                    .SetContentText(txt)
                    .SetSmallIcon(Resource.Drawable.droplet)
                    .SetContentIntent(
                        PendingIntent.GetActivity(
                            context, 0, new Intent(context, typeof(MainActivity)),
                            PendingIntentFlags.Immutable
                        )
                    ).Build();

            notifications.Cancel(NotificationId); // cancel previous one
            notifications.Notify(NotificationId, notification); // send new one
        }
    }
}
