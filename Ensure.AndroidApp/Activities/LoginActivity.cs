using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Ensure.AndroidApp.Services;
using Ensure.AndroidApp.Helpers;
using System;

namespace Ensure.AndroidApp
{
    /// <summary>
    /// Allows the user to log in with his account or navigate to register, 
    /// so as to request a password reset.
    /// </summary>
    [Activity(Label = "@string/LoginLabel")]
    public class LoginActivity : Activity, ILoadingStatedActivity
    {
        private EditText userNameEt, pwdEt;
        private Button submitBtn, registerBtn, resetBtn;
        private ProgressBar topLoadingProgress;

        private UserService userService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            userService = new UserService(this);

            userNameEt = FindViewById<EditText>(Resource.Id.LoginUserNameEt);
            pwdEt = FindViewById<EditText>(Resource.Id.LoginPasswordEt);

            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.LoginActivityTopProgress);

            submitBtn = FindViewById<Button>(Resource.Id.LoginBtnGo);
            submitBtn.Click += LoginBtnClicked;

            registerBtn = FindViewById<Button>(Resource.Id.LoginRegisterBtn);
            registerBtn.Click += RegisterBtnClicked;

            resetBtn = FindViewById<Button>(Resource.Id.LoginResetPwdBtn);
            resetBtn.Click += ResetBtn_Click;
        }

        #region Handlers
        /// <summary>
        /// Event handler for Reset Password button click
        /// opens the reset password dialog
        /// </summary>
        private void ResetBtn_Click(object sender, EventArgs e)
        {
            Dialog d = new Dialog(this);
            d.SetContentView(Resource.Layout.dialog_pwd_reset);
            d.SetCancelable(true);
            d.SetTitle("Reset Password");
            d.Create();
            d.Show();
            var btn = d.FindViewById<Button>(Resource.Id.ResetPwdDialogBtn);
            var et = d.FindViewById<EditText>(Resource.Id.ResetPwdDialogEt);
            btn.Click +=
                (s, e) => DialogResetPwdBtn_Click(d, btn, et);
        }

        /// <summary>
        /// Event handler for reset password dialog inner button click
        /// Validates the entered email & sends the reset request to the server
        /// </summary>
        /// <param name="d">The shown dialog</param>
        /// <param name="btn">The dialog's action button</param>
        /// <param name="emailEt">The dialog's edit text for the email address</param>
        private async void DialogResetPwdBtn_Click(Dialog d, Button btn, EditText emailEt)
        {
            // get typed email, validate & post
            var email = emailEt.Text;
            if (ValidationHelpers.ValidateEmail(email, this))
            {
                btn.Enabled = false; // disable modal submit button
                SetUiLoadingState(true);
                d.SetCancelable(false); // prevent dialog closing
                await userService.RequestPasswordResetEmail(email); // request password reset
                d.Cancel(); // close dialog
                SetUiLoadingState(false);
                Toast.MakeText(this, "Done. Check your inbox to continue.", ToastLength.Long).Show();
            }
            else Toast.MakeText(this, "Invalid email address", ToastLength.Long);
        }

        /// <summary>
        /// Starts the register activity
        /// </summary>
        private void RegisterBtnClicked(object sender, EventArgs e)
        {
            // Start Register activity WITH result 
            var i = new Intent(this, typeof(RegisterActivity));
            StartActivityForResult(i, (int)ActivityResults.Register);
        }

        /// <summary>
        /// Validates and logs in the user
        /// </summary>
        private async void LoginBtnClicked(object sender, EventArgs e)
        {
            // uname / password null check
            string username = userNameEt.Text, pwd = pwdEt.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwd))
            {
                return;
            }

            // try login
            SetUiLoadingState(true);
            var loginResult = await userService.LogUserIn(username, pwd);
            if (!loginResult.IsError) // login successful
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
                    .SetMessage(loginResult.ErrorMessage)
                    .SetCancelable(true)
                    .Create().Show();
            }
            SetUiLoadingState(false);
        }
        #endregion

        public void SetUiLoadingState(bool isLoading)
        {
            topLoadingProgress.Indeterminate = isLoading;
            resetBtn.Enabled = registerBtn.Enabled = submitBtn.Enabled
                = userNameEt.Enabled = pwdEt.Enabled = !isLoading;
        }

        // prevent the Finish() call by default so user won't be able to "escape" login
        public override void OnBackPressed() { }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch ((ActivityResults)requestCode)
            {
                case ActivityResults.Register:
                    // if user registerted succesfully (therefore logged in), go straight to main activity (because he's logged in!)
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