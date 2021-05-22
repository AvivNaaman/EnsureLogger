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
    /// Contains useful extension methods for the Android's <see cref="Context"/> class
    /// </summary>
    public static class ContextHelpers
    {
        /// <summary>
        /// A generic extension method to get an Android system service by it's type and service name
        /// </summary>
        /// <typeparam name="TService">The service type</typeparam>
        /// <param name="context">The context to pull the service from</param>
        /// <param name="name">The service name</param>
        /// <returns>The requested service, pulled from context and casted to <typeparamref name="TService"/></returns>
        public static TService GetSystemService<TService>(this Context context, string name)
        {
            object svc = context.GetSystemService(name);
            return (TService)svc;
        }

        /// <summary>
        /// Returns whether the application is currently being used by the user
        /// </summary>
        public static bool IsAppForeground(this Context context)
        {
            var svc = GetSystemService<ActivityManager>(context, Context.ActivityService);
            var tasks = svc.GetRunningTasks(1);
            return tasks.Count > 0 && // there is something running
                tasks.First().TopActivity.PackageName == context.PackageName; // and it's this app
        }
    }
}
