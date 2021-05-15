using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Ensure.AndroidApp.Helpers
{
	public class HttpHelper
	{
		public Uri baseUrl { get; private set; } 
		private readonly Context context;

		public HttpHelper(Context context)
		{
			this.context = context;
			baseUrl = new Uri(context.GetString(Resource.String.ApiUrl));
		}

		public HttpClient BuildClient()
		{
			var app = (EnsureApplication)context.ApplicationContext;
			HttpClient client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());

			// set base address
			client.BaseAddress = baseUrl;
			// accept json only
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
			// set auth header if user logged in
			if (app.UserInfo != null)
			{
				var bearerToken = app.UserInfo.JwtToken; // get token
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", bearerToken);
			}
			return client;
		}

		public Task<HttpResponseMessage> GetAsync(string realtiveUrl)
		{
			return BuildClient().GetAsync(realtiveUrl);
		}

		public Task<HttpResponseMessage> PostAsync(string realtiveUrl)
		{
			return PostAsync(realtiveUrl, null);
		}

		public Task<HttpResponseMessage> PostAsync(string realtiveUrl, HttpContent content)
		{
			return BuildClient().PostAsync(realtiveUrl, content);
		}
	}
}