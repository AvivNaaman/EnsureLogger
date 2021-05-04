using System;
using Android.Util;

namespace Ensure.AndroidApp.Helpers
{
    public static class LogHelper
    {
        const string Tag = "EnsureLog";

        public static int Error(string message) => Log.Error(Tag, message);

        public static int Info(string message) => Log.Info(Tag, message);
    }
}
