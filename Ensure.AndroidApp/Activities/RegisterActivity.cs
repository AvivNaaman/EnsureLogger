using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Services;
using static Ensure.AndroidApp.Helpers.ValidationHelpers;

namespace Ensure.AndroidApp
{
    /// <summary>
    /// Allows the user to create a new account.
    /// </summary>
    [Activity(Label = "@string/RegisterLabel")]
    public class RegisterActivity : AppCompatActivity, ILoadingStatedActivity
    {
        private ProgressBar topLoadingProgress;
        private Button submitBtn;
        // fields
        private EditText pwd, pwdVertification, userName, email, target;
        private UserService userService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_register);

            userService = new UserService(this);

            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.RegisterActivityTopProgress);

            submitBtn = FindViewById<Button>(Resource.Id.RegisterBtnGo);
            submitBtn.Click += SubmitBtn_Click;

            // get field references
            pwd = FindViewById<EditText>(Resource.Id.RegisterPasswordEt);
            pwdVertification = FindViewById<EditText>(Resource.Id.RegisterPasswordVertificationEt);
            userName = FindViewById<EditText>(Resource.Id.RegisterUserNameEt);
            email = FindViewById<EditText>(Resource.Id.RegisterEmailEt);
            target = FindViewById<EditText>(Resource.Id.RegisterTargetEt);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        /// <summary>
        /// Event handler for submit button's click
        /// </summary>
        private async void SubmitBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            SetUiLoadingState(true);

            if (await userService.RegiserUser(userName.Text, email.Text,
                pwd.Text, pwdVertification.Text, int.Parse(target.Text)))
            {
                SetResult(Result.Ok);
                Finish();
            }
            else
            {
                new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle("Register failed")
                    .SetMessage("One or more of the fiels were invalid.")
                    .Create().Show();
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
        
        public void SetUiLoadingState(bool isLoading)
        {
            // disabled all fields & buttons
            View[] fields = { pwd, pwdVertification, userName, target, email, submitBtn };
            fields.ToList().ForEach(f => f.Enabled = !isLoading);

            topLoadingProgress.Indeterminate = isLoading;
        }
    }

}
