using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Ensure.AndroidApp.BroadcastReceivers;
using Ensure.Domain.Models;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.Services
{
    /// <summary>
    /// A service to manage the notification scheduling and pushing.
    /// </summary>
    public class NotificationsService
    {
        private readonly Context context;

        /// <summary>
        /// Constucts a new notifications service
        /// </summary>
        public NotificationsService(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// A constant notification id (so it can be removed later)
        /// </summary>
        const int NotificationId = 1;

        /// <summary>
        /// Schedules a check for the current drinking status
        /// </summary>
        /// <param name="schedule">The next date to check on</param>
        public  void ScheduleEnsureCheckNotification(DateTime schedule)
        {
            var alarm = context.GetSystemService<AlarmManager>(Context.AlarmService);
            long scheduleAsMillisecs = (long)schedule.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            var intent = new Intent(context, typeof(EnsureNotificationReceiver));
            var pIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);

            // fire scheduling
            alarm.SetExactAndAllowWhileIdle(AlarmType.Rtc, scheduleAsMillisecs, pIntent);
        }

        public  void CancelAllScheduled()
        {
            var alarm = context.GetSystemService<AlarmManager>(Context.AlarmService);
            var intent = new Intent(context, typeof(EnsureNotificationReceiver));
            var pi = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.NoCreate);

            alarm.Cancel(pi);
        }

        /// <summary>
        /// Notifies the user to drink
        /// </summary>
        /// <param name="progress">Current progress</param>
        /// <param name="info">Current user's info</param>
        public  void NotifyEnsure(int progress, ApiUserInfo info)
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
