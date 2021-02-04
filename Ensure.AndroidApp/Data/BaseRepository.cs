using System;
using Android.Content;
using Android.Net;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.Data
{
    /// <summary>
    /// A base class for an online service, including HTTP helper
    /// and Context protected properties
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        /// The current Android context
        /// </summary>
        private protected readonly Context context;
        /// <summary>
        /// A helper class for handling HTTP Requests
        /// </summary>
        private protected readonly HttpHelper http;

        /// <summary>
        /// Constructs the service and initailizes the HTTP helper.
        /// </summary>
        /// <param name="context"></param>
        public BaseService(Context context)
        {
            this.context = context;
            this.http = new HttpHelper(context);
        }

        /// <summary>
        /// Rerturns whether the internet connection is available
        /// </summary>
        /// <returns>Whether internet connection is available</returns>
        protected bool IsInternetConnectionAvailable()
        {
            var mgr = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = mgr.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnected;
        }
    }
}
