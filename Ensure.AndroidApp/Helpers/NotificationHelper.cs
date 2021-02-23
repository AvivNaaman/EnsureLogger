using System;
using Android.OS;
using Android.Content;
using Android.App;
using Android.Support.V4.App;
using Ensure.Domain;
using AndroidX.Work;

namespace Ensure.AndroidApp.Helpers
{
    public static class NotificationHelper
    {
        const string ChannelName = "Ensure Notifications";
        const string ChannelId = "com.avivn.ensurelog.ensure_channel";
        const string ChannelDescription = "Notifications to help you keep up with your daily target.";

        const string EnsureNotificationWorkTag = "EnsureNotificationWorks";

        /// <summary>
        /// If the user's target is more than how much he drank,
        /// notify the user to drink more
        /// </summary>
        /// <param name="drank">How much the user already drank</param>
        /// <param name="target">The user's drinking target</param>
        public static void NotifyDrink(int leftToday, Context context)
        {
            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            if (leftToday > 0)
            {
                var notification = new NotificationCompat.Builder(context, ChannelId)
                    .SetContentTitle("Don't Forget to Drink!")
                    .SetContentText(string.Format("Just {0} Left today!", leftToday))
                    .Build();

                // We do that so we can cancel and find by id without the need for an extra data store
                int notificationId = int.Parse(GetDrinkNotificationId(DateTime.Today, leftToday));

                notificationManager.Notify(notificationId, notification);

            }
        }

        /// <summary>
        /// Schedules all the notifications for the specified day
        /// </summary>
        /// <param name="day"></param>
        /// <param name="target"></param>
        /// <param name="progress"></param>
        public static void ScheduleDayDrinksNotifications(Context context, DateTime day, int target)
        {
            var start = day.Date.AddHours(10); // 10 AM
            var end = start.Date.AddHours(12); // 10 PM
            long ticksToAdd = (end.Ticks - start.Ticks) / target;
            long currTicks = start.Ticks;

            var workMgr = WorkManager.Instance;

            for (int i = target; i > 0; currTicks += ticksToAdd, i++)
            {
                DateTime scheduleTime = new DateTime(currTicks);

                long millisecsDelay = (long)(scheduleTime - DateTime.Now).TotalMilliseconds;

                AndroidX.Work.Data workData = new AndroidX.Work.Data.Builder()
                    .PutInt(EnsureAndroidConstants.NotificationWorkLeftParamName, i)
                    .Build();

                OneTimeWorkRequest request = new OneTimeWorkRequest.Builder(typeof(NotificationWorker))
                    .AddTag(EnsureNotificationWorkTag)
                    .SetInputData(workData)
                    .SetInitialDelay(millisecsDelay, Java.Util.Concurrent.TimeUnit.Milliseconds)
                    .Build();

                workMgr.BeginUniqueWork(
                    GetDrinkNotificationId(scheduleTime, i),
                    ExistingWorkPolicy.Replace, request);
            }
        }

        public static void RemoveAllDrinkingNotifications(int lastTarget, Context context, DateTime day)
        {
            // cancel ALL scheduled notifications by tag
            WorkManager.Instance.CancelAllWorkByTag(EnsureNotificationWorkTag);
        }

        /// <summary>
        /// Returns the corresponding notification id to the date, target and progress.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="drank"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetDrinkNotificationId(DateTime day, int drinksLeft)
            => $"{day.Day.ToString("yyyyMMdd")}{drinksLeft}";

        /// <summary>
        /// Registers a notification service
        /// </summary>
        public static void CreateChaneelIfRequired(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = GetNotificationManager(context);
                var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High);
                channel.Description = ChannelDescription;
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        /// <summary>
        /// Gets the context's notification manager
        /// </summary>
        public static NotificationManager GetNotificationManager(Context context)
            => (NotificationManager)context.GetSystemService(Context.NotificationService);
    }
}
