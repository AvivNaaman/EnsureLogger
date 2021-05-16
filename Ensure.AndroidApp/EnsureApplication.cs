using Android.App;
using Android.Runtime;
using Ensure.Domain.Models;
using System;

namespace Ensure.AndroidApp
{
	/// <summary>
    /// Holds the global application state, so as defines important data.
    /// </summary>
	[Application(
		Debuggable =
#if DEBUG
		true,
		NetworkSecurityConfig = "@xml/net_config" // disable network security policy (TLS) when debugging
#else
		false
#endif
		)
		]
	
	class EnsureApplication : Application
	{
		/// <summary>
        /// Hold the currently logged on user info
        /// </summary>
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