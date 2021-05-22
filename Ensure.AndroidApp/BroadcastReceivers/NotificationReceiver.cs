using Android.Content;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Services;
using System;

namespace Ensure.AndroidApp.BroadcastReceivers
{
    /// <summary>
    /// A Helper BroadcastReceiver to handle delayed notification intents,
    /// coming from NotificationHelper.ScheduleNotification.
    /// </summary>
    [BroadcastReceiver]
    public class EnsureNotificationReceiver : BroadcastReceiver
    {
        /// <inheritdoc/>
        public override async void OnReceive(Context context, Intent intent)
        {
            LogHelper.Info("Notification receiver called!");

            try
            {
                var notificationService = new NotificationsService(context);
                await notificationService.HandleNotificationBroadcastAsync(intent);
            }
            catch (Exception e)
            {
                LogHelper.Error($"ERROR! Message: `{e.Message}` StackTrace: `{e.StackTrace}`");
            }
        }
    }
}
