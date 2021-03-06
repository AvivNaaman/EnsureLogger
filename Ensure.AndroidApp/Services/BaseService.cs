using System;
using System.Net.Http;
using System.Security.Authentication;
using Android.Content;
using Android.Net;
using Android.Support.V7.App;
using Android.Util;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.Services
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
            http = new HttpHelper(context);
        }

        /// <summary>
        /// Rerturns whether the internet connection is available
        /// </summary>
        /// <returns>Whether internet connection is available</returns>
        protected bool IsInternetConnectionAvailable()
        {
            var mgr = context.GetSystemService<ConnectivityManager>(Context.ConnectivityService);
            NetworkInfo netInfo = mgr.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnected;
        }



        /// <summary>
        /// Handles an HTTP error, thrown by the HttpClient used by the repository.
        /// </summary>
        /// <param name="message"></param>
        protected void HandleHttpError(HttpResponseMessage message, bool showErrorDialog)
        {
            // auth exception - throw up.
            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }
            // general http error - show alert
            else if (showErrorDialog)
            {
                new AlertDialog.Builder(context)
                    .SetMessage($"HTTP Error {message.StatusCode} - {message.ReasonPhrase}")
                    .SetTitle("HTTP Error")
                    .Create().Show();
            }
            LogHelper.Error($"HTTP Error {message.StatusCode} - {message.ReasonPhrase}");
        }
    }
}
