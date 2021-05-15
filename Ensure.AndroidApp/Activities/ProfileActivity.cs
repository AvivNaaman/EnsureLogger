using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Services;

namespace Ensure.AndroidApp
{
    [Activity(Label = "Profile & Account")]
    public class ProfileActivity : AppCompatActivity, ILoadingStatedActivity
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

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            // load UI
            UpdateFromInfo();
        }

        /// <summary>
        /// Event handler for save button click - posts the new infomation
        /// </summary>
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
                int prevTarget = userService.CurrentUser.DailyTarget;
                if (!await userService.SetUserTarget(parsedTarget)) 
                {
                    target.Text = prevTarget.ToString(); // error
                }
            }
            SetUiLoadingState(false);
        }

        /// <summary>
        /// Loads the user info into the UI from the local cache
        /// </summary>
        private void UpdateFromInfo()
        {
            var info = userService.CurrentUser;
            userName.Text = info.UserName;
            email.Text = info.Email;
            target.Text = info.DailyTarget.ToString();
        }

        public void SetUiLoadingState(bool isLoading)
        {
            topProgress.Indeterminate = isLoading;
            saveTargetBtn.Enabled = target.Enabled = !isLoading;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    SetResult(Result.Canceled);
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}
