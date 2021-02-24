using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ensure.Domain.Entities;
using System.Collections.Generic;
using Ensure.Domain.Enums;
using System;
using System.Linq;
using Android.Util;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Ensure.AndroidApp.Helpers;
using Ensure.AndroidApp.Adapters;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Ensure.Domain;
using Ensure.Domain.Models;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.BroadcastReceivers;
using System.Security.Authentication;

namespace Ensure.AndroidApp
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        // TODO: Move to smart props!
        private List<InternalEnsureLog> ensures = new List<InternalEnsureLog>();

        private ProgressBar horizontalTopProgress;
        private Spinner tasteSpinner;
        private Button addBtn;
        private SwipeRefreshLayout refreshLayout;

        private TextView helloUserTv;

        private EnsureRepository ensureService;
        private UserService userService;

        private ProgressBar todayProgress;
        private TextView todayProgressTv;

        private int currentProgress;

        private NetStateReceiver netStateReceiver;

        private ImageButton historyBtn, profileBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            // Hello, UserName message
            helloUserTv = FindViewById<TextView>(Resource.Id.HelloUserTv);

            // Taste picker
            tasteSpinner = FindViewById<Spinner>(Resource.Id.EnsureTasteSpinner);
            var tastesList = Enum.GetValues(typeof(EnsureTaste)).Cast<EnsureTaste>().ToList();
            tasteSpinner.Adapter = new EnsureTasteSpinnerAdapter(this, tastesList);

            // Add button
            addBtn = FindViewById<Button>(Resource.Id.addBtn);
            addBtn.Click += AddBtn_Click;

            // Top progress bar
            horizontalTopProgress = FindViewById<ProgressBar>(Resource.Id.MainActivityTopProgress);

            // Swipe to refresh layout
            refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.MainActivityRefreshableLayout);
            refreshLayout.Refresh += MainActivity_Refresh;

            todayProgress = FindViewById<ProgressBar>(Resource.Id.TodayProgressBar);
            todayProgressTv = FindViewById<TextView>(Resource.Id.MainProgressTv);

            historyBtn = FindViewById<ImageButton>(Resource.Id.HistoryImageBtn);
            historyBtn.Click += HistoryBtn_Click;
            profileBtn = FindViewById<ImageButton>(Resource.Id.ProfileImageBtn);
            profileBtn.Click += ProfileBtn_Click;

        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(ProfileActivity));
            StartActivity(i);
        }

        private void HistoryBtn_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(HistoryActivity));
            StartActivity(i);
        }

        /// <summary>
        /// Swipe refresh event handler
        /// </summary>
        private async void MainActivity_Refresh(object sender, EventArgs e)
        {
            try
            {
                await RefreshTargetProgress();
            }
            catch (AuthenticationException)
            {
                StartLoginActivity();
            }
            refreshLayout.Refreshing = false;
        }

        /// <summary>
        /// Add button event handler
        /// </summary>
        private async void AddBtn_Click(object sender, EventArgs e)
        {
            SetUiLoadingState(true);
            // add to server (if online) & cache
            try
            {
                var result = await ensureService.AddLogAsync((EnsureTaste)tasteSpinner.SelectedItemPosition);
                if (result != null)
                {
                    Toast.MakeText(this, "Log Added", ToastLength.Long).Show();
                    currentProgress++;
                    UpdateTargetUi();
                }
            }
            catch (AuthenticationException)
            {
                StartLoginActivity();
            }
            SetUiLoadingState(false);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Refreshes the Target and today's progress
        /// </summary>
        private async Task RefreshTargetProgress()
        {
            SetUiLoadingState(true);
            // refresh today's logs (+history)
            await userService.RefreshInfo();
            var logs = await ensureService.GetLogs(DateTime.MinValue);
            currentProgress = logs.Count;
            UpdateTargetUi();
            SetUiLoadingState(false); return;
        }

        /// <summary>
        /// Updates the target and the progress of today 
        /// </summary>
        private void UpdateTargetUi()
        {
            var target = userService?.UserInfo?.DailyTarget;
            if (!target.HasValue || target < 0) target = 0;
            todayProgressTv.Text = currentProgress + "/" + target.Value;
            todayProgress.Progress = currentProgress;
            todayProgress.Max = target.Value;
        }

        /// <summary>
        /// Starts the login activity
        /// </summary>
        private void StartLoginActivity()
        {
            var intent = new Intent(this, typeof(LoginActivity));
            StartActivityForResult(intent, (int)ActivityRequestCodes.Login);
        }

        /// <summary>
        /// Changed the UI to match the loading state
        /// </summary>
        /// <param name="isLoading"></param>
        private void SetUiLoadingState(bool isLoading)
        {
            horizontalTopProgress.Indeterminate = isLoading; // "disabled"
            refreshLayout.Enabled = addBtn.Enabled = !isLoading;
        }

        /* Options Menu */
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_logout:
                    Logout();
                    break;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Logs the user out and turns him to the login
        /// </summary>
        private void Logout()
        {
            SetUiLoadingState(true);
            StartLoginActivity(); // exit to login activity
            userService.LogUserOut();
            ensures.Clear(); // Remove ensures & any other previous user data on UI
            todayProgress.Progress = 0;
            SetUiLoadingState(false);
        }

        protected override void OnResume()
        {
            // init services & load data
            ensureService = new EnsureRepository(this);
            userService = new UserService(this);
            userService.LoadUserInfoFromSp();

            // register network broadcast receiver
            netStateReceiver = new NetStateReceiver();
            IntentFilter netFilter = new IntentFilter(Android.Net.ConnectivityManager.ConnectivityAction);
            netStateReceiver.NetworkStateChanged += NetworkStateChanged;
            RegisterReceiver(netStateReceiver, netFilter);
            base.OnResume();

            if (userService.IsLoggedIn)
            {
                helloUserTv.Text = $"Hello, {userService.UserInfo.UserName}";
                netStateReceiver.NetworkStateChanged -= NetworkStateChanged;
                netStateReceiver.NetworkStateChanged += NetworkStateChanged;
            }
            UpdateTargetUi();
        }

        /// <summary>
        /// Network state changes event handler
        /// </summary>
        /// <param name="isNetConnected"></param>
        private async void NetworkStateChanged(bool isNetConnected, bool prevState)
        {
            if (prevState != isNetConnected) // connectivity (connected/not connected) changed
            {
                if (isNetConnected) // offline -> online: SYNC!
                {
                    SetUiLoadingState(true);
                    try
                    {
                        await ensureService.SyncEnsures();
                        await RefreshTargetProgress();
                    }
                    catch (AuthenticationException)
                    {
                        StartLoginActivity();
                    }
                    SetUiLoadingState(false);
                }
                else // online -> offline
                {

                }
            }
        }

        protected override void OnPause()
        {
            UnregisterReceiver(netStateReceiver);
            netStateReceiver.Dispose();
            netStateReceiver = null;
            base.OnPause();
        }

        enum ActivityRequestCodes
        {
            Login
        }

    }
}
