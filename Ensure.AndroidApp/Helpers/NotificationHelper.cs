using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Ensure.AndroidApp.BroadcastReceivers;
using Ensure.Domain.Models;

namespace Ensure.AndroidApp.Helpers
{
    /// <summary>
    /// Contains helper methods for handling the notification
    /// management and scheduling in the app
    /// </summary>
    public static class NotificationHelper
    {
        const int NotificationId = 1;

        /// <summary>
        /// Schedules a check for the current drinking status
        /// </summary>
        /// <param name="schedule">The next date to check on</param>
        public static void ScheduleEnsureCheckNotification(Context context, DateTime schedule)
        {
            var alarm = (AlarmManager)context.GetSystemService(Context.AlarmService);
            long scheduleAsMillisecs = (long)schedule.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            var intent = new Intent(context, typeof(EnsureNotificationReceiver));
            var pIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);

            // fire scheduling
            alarm.SetExactAndAllowWhileIdle(AlarmType.Rtc, scheduleAsMillisecs, pIntent);
        }

        /// <summary>
        /// Notifies the user to drink
        /// </summary>
        /// <param name="progress">Current progress</param>
        /// <param name="info">Current user's info</param>
        public static void NotifyEnsure(Context context, int progress, ApiUserInfo info)
        {
            var notifications = (NotificationManager)context.GetSystemService(Context.NotificationService);
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
                    .SetSmallIcon(Resource.Drawable.notification_action_background)
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
