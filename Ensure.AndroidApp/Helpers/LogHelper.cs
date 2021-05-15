using System;
using Android.Util;

namespace Ensure.AndroidApp.Helpers
{
    /// <summary>
    /// A helper class for logging actions and events in the logcat.
    /// </summary>
    public static class LogHelper
    {
        const string Tag = "EnsureLog";

        /// <summary>
        /// Logs an error message to the logcat
        /// </summary>
        public static int Error(string message) => Log.Error(Tag, message);

        /// <summary>
        /// Logs an information message to the logcat
        /// </summary>
        public static int Info(string message) => Log.Info(Tag, message);
    }
}
