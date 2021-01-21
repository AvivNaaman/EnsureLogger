using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Helpers;
using Ensure.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Ensure.AndroidApp
{
	[Activity(Label = "LoginActivity")]
	public class LoginActivity : Activity
	{
		private EditText userNameEt, pwdEt;
		private Button submitBtn, registerBtn;
		private ProgressBar topLoadingProgress;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.login_layout);

			userNameEt = FindViewById<EditText>(Resource.Id.LoginUserNameEt);
			pwdEt = FindViewById<EditText>(Resource.Id.LoginPasswordEt);

			topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.LoginActivityTopProgress);

			submitBtn = FindViewById<Button>(Resource.Id.LoginBtnGo);
			submitBtn.Click += LoginBtnClicked;

			registerBtn = FindViewById<Button>(Resource.Id.LoginRegisterBtn);
			registerBtn.Click += RegisterBtnClicked;
		}

		private void RegisterBtnClicked(object sender, EventArgs e)
        {
			// Start Register activity
			var i = new Intent(this, typeof(RegisterActivity));
			StartActivityForResult(i, (int)ActvityResults.Register);
        }

		private async void LoginBtnClicked(object sender, EventArgs e)
		{
			Log.Debug("LoginActivity", "btn clicked");
			string username = userNameEt.Text, pwd = pwdEt.Text;
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwd))
			{
				return;
			}
			try
			{
				SetUiLoadingState(true);

				// Fetch
				var http = new HttpHelper(this);
				var res = await http.GetAsync($"/api/Account/login?username={username}&password={pwd}");

				if (!res.IsSuccessStatusCode)
				{
					if (res.StatusCode.HasFlag(System.Net.HttpStatusCode.BadRequest)) // Auth failed
						throw new AuthenticationException("User name and password do not match.");
					else throw new AuthenticationException($"Error: Status code {res.StatusCode} does not indicate success.");
				}
				string content = await res.Content.ReadAsStringAsync();
				// Json
				try
				{
					Log.Debug("Login", $"Hello {content} END");
					var userInfo = JsonConvert.DeserializeObject<ApiUserInfo>(content);
					// Handle response
					if (userInfo != null)
					{
						// logged in successfully! get back to MainActivity
						((EnsureApplication)ApplicationContext).UpdateUserInfo(userInfo);
						SetResult(Result.Ok);
						Finish();
						return;
					}
					else
					{
						throw new AuthenticationException("Authentication failed.");
					}

				}
				catch (JsonException jEx)
				{
					Log.Error("JsonLogin", $"Json Parse failed. response: {content}, message: {jEx.Message}");
					throw new AuthenticationException("Error: Failed to parse response from server.");
				}
			}
			// something failed
			catch (AuthenticationException aEx)
			{
				// Remove Password
				pwdEt.Text = String.Empty;
				// Show error alert
				var builder = new AlertDialog.Builder(this);
				builder.SetTitle("Login failed");
				builder.SetMessage(aEx.Message);
				builder.SetPositiveButton("OK", (e, sender) => { });
				builder.Create().Show();
				SetUiLoadingState(false);
			}
		}

		private void SetUiLoadingState(bool isLoading)
		{
			topLoadingProgress.Indeterminate = isLoading;
			submitBtn.Clickable = submitBtn.Enabled = submitBtn.Focusable = userNameEt.Enabled = pwdEt.Enabled = !isLoading;
		}

		public override void OnBackPressed()
		{
			// Prevent user from leaving activity - do nothing.
		}


		/// <summary>
		/// The Activity Results for the current activity
		/// </summary>
		private enum ActvityResults
		{
			Register
		};
	}
}