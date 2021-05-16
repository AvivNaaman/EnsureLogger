using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Ensure.AndroidApp.BroadcastReceivers;
using Ensure.Domain.Models;

namespace Ensure.AndroidApp.Helpers
{
    public static class ContextHelpers
    {
        public static TService GetSystemService<TService>(this Context context, string service)
        {
            object svc = context.GetSystemService(service);
            return (TService)svc;
        }
    }
}
