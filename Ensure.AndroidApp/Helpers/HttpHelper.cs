using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ensure.AndroidApp.Helpers
{
	// TODO: Move to GetJsonAsync<TResult> & PostJsonAsync<TResult>
	public class HttpHelper
	{
		public Uri BaseUrl { get; } = new Uri("https://ensurelog.azurewebsites.net");
		private readonly Context context;

		public HttpHelper(Context context)
		{
			this.context = context;
		}

		public HttpClient BuildClient()
		{

			HttpClient client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());
			var app = ((EnsureApplication)context.ApplicationContext);
			if (app.UserInfo != null)
			{
				var bearerToken = app.UserInfo.JwtToken;
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", bearerToken);
			}
			return client;
		}

		public Task<HttpResponseMessage> GetAsync(string realtiveUrl)
		{
			return BuildClient().GetAsync(new Uri(BaseUrl, realtiveUrl));
		}

		public Task<HttpResponseMessage> PostAsync(string realtiveUrl)
		{
			return PostAsync(realtiveUrl, null);
		}

		public Task<HttpResponseMessage> PostAsync(string realtiveUrl, HttpContent content)
		{
			return BuildClient().PostAsync(new Uri(BaseUrl, realtiveUrl), content);
		}
	}
}