using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ensure.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ensure.AndroidApp
{
	[Application]
	class EnsureApplication : Application
	{
		const string SharedPrefernceName = "EnsureSp";
		const string UserInfoSharedPreference = "UserInfo";
		public ApiUserInfo UserInfo { get; private set; }
		public bool IsLoggedIn { get { return UserInfo != null; } }
		private ISharedPreferences _sharedPreferences;
		public EnsureApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
		public override void OnCreate()
		{
			base.OnCreate();
			_sharedPreferences = GetSharedPreferences(SharedPrefernceName, FileCreationMode.Private);
			// Try to get user's info
			var userInfoJson = _sharedPreferences.GetString(UserInfoSharedPreference, String.Empty);
			Log.Debug("EnsureApp", $"Hello! {userInfoJson}");
			if (!String.IsNullOrEmpty(userInfoJson))
			{
				UserInfo = JsonConvert.DeserializeObject<ApiUserInfo>(userInfoJson);
			}
		}
		public void UpdateUserInfo(ApiUserInfo info)
		{
			if (info == null) throw new NullReferenceException("User info cannot be null.");
			else
			{
				UserInfo = info;
				// Save UserInfo as json string to SharedPreferences
				var json = JsonConvert.SerializeObject(info);
				_sharedPreferences.Edit().PutString(UserInfoSharedPreference, json).Commit();
			}
		}
		public void LogUserOut()
		{
			UserInfo = null;
			_sharedPreferences.Edit().PutString(UserInfoSharedPreference, String.Empty).Commit();
		}
	}
}