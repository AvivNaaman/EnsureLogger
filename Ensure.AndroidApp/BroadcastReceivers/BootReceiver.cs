
using System;
using Android.App;
using Android.Content;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Services;
using static Android.Manifest;

namespace Ensure.AndroidApp.BroadcastReceivers
{
    /// <summary>
    /// Receivs and handles the device's power on event -
    /// starts the notification scheduler
    /// </summary>
    [BroadcastReceiver(Enabled = true, Exported = false, Permission = Permission.ReceiveBootCompleted)]
    [IntentFilter(new string[] { Intent.ActionBootCompleted })] // auto register to power event
    public class BootReceiver : BroadcastReceiver
    {
        /// <inheritdoc/>
        public override void OnReceive(Context context, Intent intent)
        {
            LogHelper.Info("Boot Completed and boot event fired!");
            // schedule next notification soon but not too soon
            NotificationsService ns = new(context);
            ns.ScheduleEnsureCheckNotification(DateTime.Now.AddMinutes(1), true);
        }
    }
}
