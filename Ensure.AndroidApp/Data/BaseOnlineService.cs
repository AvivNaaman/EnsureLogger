using System;
using Android.Content;
using Android.Net;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.Data
{
    public abstract class BaseOnlineService
    {
        private protected readonly Context context;
        private protected readonly HttpHelper http;

        public BaseOnlineService(Context context)
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
