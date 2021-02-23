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
		public ApiUserInfo UserInfo { get; set; }
		public EnsureApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}