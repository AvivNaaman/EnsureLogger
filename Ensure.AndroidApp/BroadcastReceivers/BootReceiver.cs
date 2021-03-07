
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.Helpers;
using static Android.Manifest;

namespace Ensure.AndroidApp.BroadcastReceivers
{
    /// <summary>
    /// A broadcast receiver which fires on phone start,
    /// and re-starts the notification alarms
    /// </summary>
    [BroadcastReceiver(Enabled = true, Exported = false, Permission = Permission.ReceiveBootCompleted)]
    [IntentFilter(new string[] { Intent.ActionBootCompleted })] // auto register to power event
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info("EnsureLogger", "Boot Completed and ensure event fired!");
            NotificationHelper.ScheduleEnsureCheckNotification(context, DateTime.Now.AddMinutes(1));
        }
    }
}
