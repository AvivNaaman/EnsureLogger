
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Helpers;
using Ensure.Domain.Models;
using Newtonsoft.Json;
using static Ensure.AndroidApp.Helpers.ValidationHelpers;

namespace Ensure.AndroidApp
{
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        private ProgressBar topLoadingProgress;
        private Button submitBtn, loginInsteadBtn;
        private EditText pwd, pwdVertification, userName, email, target;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_register);

            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.RegisterActivityTopProgress);

            submitBtn = FindViewById<Button>(Resource.Id.RegisterBtnGo);
            submitBtn.Click += SubmitBtn_Click;

            loginInsteadBtn = FindViewById<Button>(Resource.Id.RegisterLoginBtn);
            loginInsteadBtn.Click += LoginInsteadBtn_Click;

            // fields
            pwd = FindViewById<EditText>(Resource.Id.RegisterPasswordEt);
            pwdVertification = FindViewById<EditText>(Resource.Id.RegisterPasswordVertificationEt);
            userName = FindViewById<EditText>(Resource.Id.RegisterUserNameEt);
            email = FindViewById<EditText>(Resource.Id.RegisterEmailEt);
            target = FindViewById<EditText>(Resource.Id.RegisterTargetEt);
        }

        private void LoginInsteadBtn_Click(object sender, EventArgs e)
        {
            // Cancel and go back to login
            SetResult(Result.Canceled);
            Finish();
        }

        private async void SubmitBtn_Click(object sender, EventArgs e)
        {
            // validate first
            if (!ValidateForm()) return;

            SetUiLoadingState(true);

            var model = new ApiSignupModel
            {
                Password = pwd.Text,
                PasswordVertification = pwdVertification.Text,
                UserName = userName.Text,
                Email = email.Text,
                DailyTarget = short.Parse(target.Text)
            };

            // Post To Server
            string data = JsonConvert.SerializeObject(model);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var res = await new HttpHelper(this).PostAsync("/api/Account/Register", content);

            if (!res.IsSuccessStatusCode)
            {
                ShowErrorDialog("Registartion failed.", this);
            }
            else
            {
                var userInfo = JsonConvert.DeserializeObject<ApiUserInfo>
                                (await res.Content.ReadAsStringAsync());
                ((EnsureApplication)Application).UpdateUserInfo(userInfo);
                SetResult(Result.Ok); // success!
                Finish();
            }

            SetUiLoadingState(false);
        }

        /// <summary>
        /// Validates the registration form, returns whether it's valid
        /// and displays a dialog if not valid with the error message.
        /// </summary>
        /// <returns>Whether the form is valid</returns>
        private bool ValidateForm()
        {
            // validate fields: null check + password match + email + password security
            return ValidateFilled(this, userName.Text, pwd.Text,
                pwdVertification.Text, email.Text, target.Text) &&
                pwd.Text == pwdVertification.Text &&
                ValidateEmail(email.Text, this) && ValidatePassword(pwd.Text, this);
        }

        private void SetUiLoadingState(bool isLoading)
        {
            // disabled all fields & buttons
            View[] fields = { pwd, pwdVertification, userName, target, email, submitBtn, loginInsteadBtn };
            fields.ToList().ForEach(f => f.Enabled = !isLoading);

            topLoadingProgress.Indeterminate = isLoading;
        }
    }

}
