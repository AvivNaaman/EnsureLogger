using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Ensure.AndroidApp.BroadcastReceivers;
using Ensure.Domain.Models;

namespace Ensure.AndroidApp.Helpers
{
    public static class NotificationHelper
    {
        public static void ScheduleEnsureCheckNotification(Context context, DateTime schedule)
        {
            var alarm = (AlarmManager)context.GetSystemService(Context.AlarmService);
            long scheduleAsMillisecs = (long)schedule.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var intent = new Intent(context, typeof(EnsureNotificationReceiver));
            // TODO: Inspect request code?
            var pIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.OneShot);
            // fire scheduling
            alarm.SetExactAndAllowWhileIdle(AlarmType.Rtc, scheduleAsMillisecs, pIntent);
        }

        public static void NotifyEnsure(Context context, int progress, ApiUserInfo info)
        {
            var notifications = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var cid = context.GetString(Resource.String.EnsureNotificationChannelId);
            var cname = context.GetString(Resource.String.EnsureNotificationChannelName);
            var cdesc = context.GetString(Resource.String.EnsureNotificationChannelDesc);

            string title = "Drink Notification";
            string txt = $"Don't forget to drink! You're currently in { progress } out of { info.DailyTarget } today!";


            Notification notification;
            if (BuildVersionCodes.O <= Build.VERSION.SdkInt)
            {
                // Create channel for Oreo+
                notifications.CreateNotificationChannel(new NotificationChannel(cid, cname, NotificationImportance.High) { Description = cdesc });
                notification = new Notification.Builder(context, cid)
                    .SetContentTitle(title)
                    .SetContentText(txt)
                    .SetSmallIcon(Resource.Drawable.notification_action_background)
                    .SetContentIntent(
                        PendingIntent.GetActivity(
                            context, 0, new Intent(context, typeof(MainActivity)),
                            PendingIntentFlags.Immutable
                        )
                    )
                    .Build();
            }
            else // legacy notification
            {
                notification = new Notification.Builder(context)
                    .SetContentTitle(title)
                    .SetContentText(txt)
                    .Build();
            }

            // TODO: Change Id?
            notifications.Cancel(12345);
            notifications.Notify(12345, notification);
        }
    }
}
