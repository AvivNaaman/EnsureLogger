using System;
using Android.Content;
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
        public override async void OnReceive(Context context, Intent intent)
        {
            // just call NotificationHelper.Notify(,,)
            var ensureRepository = new EnsureRepository(context);
            var userSvc = new UserService(context);
            // Refresh & Notify TODO: only if needed!
            var userInfo = await userSvc.RefreshInfo();
            var progress = (await ensureRepository.GetLogs(DateTime.MinValue)).Count;
            NotificationHelper.NotifyEnsure(context, progress, userInfo);
        }
    }
}
