using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Data;
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

        private UserService userService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login_layout);

            userService = new UserService(this);

            userNameEt = FindViewById<EditText>(Resource.Id.LoginUserNameEt);
            pwdEt = FindViewById<EditText>(Resource.Id.LoginPasswordEt);

            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.LoginActivityTopProgress);

            submitBtn = FindViewById<Button>(Resource.Id.LoginBtnGo);
            submitBtn.Click += LoginBtnClicked;

            registerBtn = FindViewById<Button>(Resource.Id.LoginRegisterBtn);
            registerBtn.Click += RegisterBtnClicked;
        }

        /// <summary>
        /// Starts the register activity
        /// </summary>
        private void RegisterBtnClicked(object sender, EventArgs e)
        {
            // Start Register activity
            var i = new Intent(this, typeof(RegisterActivity));
            StartActivityForResult(i, (int)ActivityResults.Register);
        }

        /// <summary>
        /// Validates and logs in the user
        /// </summary>
        private async void LoginBtnClicked(object sender, EventArgs e)
        {
            string username = userNameEt.Text, pwd = pwdEt.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwd))
            {
                return;
            }

            SetUiLoadingState(true);
            if (await userService.LogUserIn(username, pwd)) // login successful
            {
                SetResult(Result.Ok);
                Finish();
            }
            else // login failed
            {
                // Remove Password
                pwdEt.Text = string.Empty;
                // Show error alert
                new AlertDialog.Builder(this)
                    .SetTitle("Login failed")
                    .SetMessage("One or more of the credentials were wrong.")
                    .Create().Show();
            }
            SetUiLoadingState(false);
        }

        /// <summary>
        /// Sets the loading state for the UI
        /// </summary>
        /// <param name="isLoading">Should UI indicate loading state</param>
        private void SetUiLoadingState(bool isLoading)
        {
            topLoadingProgress.Indeterminate = isLoading;
            submitBtn.Clickable = submitBtn.Enabled = submitBtn.Focusable = userNameEt.Enabled = pwdEt.Enabled = !isLoading;
        }

        // just prevent the Finish() call on default
        public override void OnBackPressed() { }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch ((ActivityResults)requestCode)
            {
                case ActivityResults.Register:
                    // if user registerted succesfully, go home
                    if (resultCode == Result.Ok)
                    {
                        Finish();
                    }
                    break;
                default:
                    break;
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        /// <summary>
        /// The Activity Results for the current activity
        /// </summary>
        private enum ActivityResults
        {
            Register
        };
    }
}