
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
using Ensure.AndroidApp.Data;
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

        private UserService userService;

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

            userService = new UserService(this);

            UpdateFromInfo();
        }

        private async void SaveTargetBtn_Click(object sender, EventArgs e)
        {
            SetUiLoadingState(true);
            short parsedTarget;
            if (string.IsNullOrWhiteSpace(target.Text) || (parsedTarget = short.Parse(target.Text)) <= 0)
            {
                ValidationHelpers.ShowErrorDialog("Please enter a valid target.", this);
            }
            else
            {
                try
                {
                    short prevTarget = userService.UserInfo.DailyTarget;
                    if (!await userService.SetUserTarget(parsedTarget))
                    {
                        target.Text = prevTarget.ToString(); // error
                    }
                }
                catch
                {
                    // TODO: Back to login
                }
            }
            SetUiLoadingState(false);
        }

        private void UpdateFromInfo()
        {
            var info = userService.UserInfo;
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
