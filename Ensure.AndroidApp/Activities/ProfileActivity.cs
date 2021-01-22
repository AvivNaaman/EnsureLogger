
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp
{
    [Activity(Label = "ProfileActivity")]
    public class ProfileActivity : Activity
    {
        private TextView userName, email;
        private EditText target;
        private Button saveTargetBtn;
        private ProgressBar topProgress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_profile);
            // Create your application here
            userName = FindViewById<TextView>(Resource.Id.ProfileUserNameTv);
            email = FindViewById<TextView>(Resource.Id.ProfileEmailTv);
            target = FindViewById<EditText>(Resource.Id.ProfileTargetEt);

            saveTargetBtn = FindViewById<Button>(Resource.Id.ProfileSaveButton);
            saveTargetBtn.Click += SaveTargetBtn_Click;

            topProgress = FindViewById<ProgressBar>(Resource.Id.ProfileActivityTopProgress);

            UpdateFromInfo();
        }

        private async void SaveTargetBtn_Click(object sender, EventArgs e)
        {
            SetUiLoadingState(true);
            int parsedTarget;
            if (string.IsNullOrWhiteSpace(target.Text) || (parsedTarget = int.Parse(target.Text)) <= 0)
            {
                ValidationHelpers.ShowErrorDialog("Please enter a valid target.", this);
            }
            else
            {
                var res = await new HttpHelper(this).PostAsync("/api/Account/SetTarget?target={parsedTarget}");
                if (!res.IsSuccessStatusCode)
                {
                    // TODO: Make an error
                }
                else
                {
                    Toast.MakeText(this, "Saved!", ToastLength.Long).Show();
                }
            }
            SetUiLoadingState(false);
        }

        private void UpdateFromInfo()
        {
            var app = (EnsureApplication)Application;
            var info = app.UserInfo;
            userName.Text = info.UserName;
            email.Text = info.Email;
            target.Text = info.DailyTarget.ToString();
        }

        private void SetUiLoadingState(bool isLoading)
        {
            topProgress.Indeterminate = isLoading;
            saveTargetBtn.Enabled = target.Enabled = !isLoading;
        }
    }
}
