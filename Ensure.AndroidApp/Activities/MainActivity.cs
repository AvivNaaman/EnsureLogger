using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ensure.Domain.Enums;
using System;
using System.Linq;
using Android.Support.V4.Widget;
using Ensure.AndroidApp.Adapters;
using Android.Views;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.BroadcastReceivers;
using System.Security.Authentication;
using Ensure.AndroidApp.Services;

namespace Ensure.AndroidApp
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ILoadingStatedActivity
    {
        private List<InternalEnsureLog> ensures = new List<InternalEnsureLog>();

        #region WidgetRefernces
        // shell UI (swipe refresh & progress bar)
        private ProgressBar topLoadingProgress;
        private SwipeRefreshLayout mainLayout;

        // quick add UI
        private Spinner tasteSpinner;
        private Button addBtn;

        // user "Hello" message
        private TextView topHelloMessage;

        // service
        private EnsureRepository ensureRepo;
        private UserService userService;

        // progress bar & inner text
        private ProgressBar mainProgressBar;
        private TextView todayProgressTv;

        private int todayProgress; // current progress

        private NetStateReceiver netStateReceiver; // network state listener

        private ImageButton historyBtn, profileBtn; // profile/history image buttons
        #endregion

        #region Lifecycle
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            // Hello, UserName message
            topHelloMessage = FindViewById<TextView>(Resource.Id.HelloUserTv);

            // Taste picker
            tasteSpinner = FindViewById<Spinner>(Resource.Id.EnsureTasteSpinner);
            var tastesList = Enum.GetValues(typeof(EnsureTaste)).Cast<EnsureTaste>().ToList();
            tasteSpinner.Adapter = new EnsureTasteSpinnerAdapter(this, tastesList);

            // Add button
            addBtn = FindViewById<Button>(Resource.Id.addBtn);
            addBtn.Click += AddBtn_Click;

            // Top progress bar
            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.MainActivityTopProgress);

            // Swipe to refresh layout
            mainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.MainActivityRefreshableLayout);
            mainLayout.Refresh += MainActivity_Refresh;

            mainProgressBar = FindViewById<ProgressBar>(Resource.Id.TodayProgressBar);
            todayProgressTv = FindViewById<TextView>(Resource.Id.MainProgressTv);

            historyBtn = FindViewById<ImageButton>(Resource.Id.HistoryImageBtn);
            historyBtn.Click += HistoryBtn_Click;
            profileBtn = FindViewById<ImageButton>(Resource.Id.ProfileImageBtn);
            profileBtn.Click += ProfileBtn_Click;

        }

        protected override async void OnResume()
        {
            base.OnResume();
            // init services & load data
            ensureRepo = new EnsureRepository(this);
            userService = new UserService(this);
            userService.LoadUserInfoFromSp();

            if (!userService.IsUserLoggedIn)
            {
                StartLoginActivity();
                return;
            }

            // update logs & target & progress - force use of cache for nice start (won't block for long, I hope)
            var l = await ensureRepo.GetLogs(DateTime.MinValue, true);
            todayProgress = l.Count; // exclude all the requires deletion ones
            UpdateProgressDiaplay();

            // register network broadcast receiver
            netStateReceiver = new NetStateReceiver();
            IntentFilter netFilter = new IntentFilter(Android.Net.ConnectivityManager.ConnectivityAction);
            netStateReceiver.NetworkStateChanged += NetworkStateChanged;
            RegisterReceiver(netStateReceiver, netFilter);

            topHelloMessage.Text = $"Hello, {userService.CurrentUser.UserName}";
        }

        protected override void OnPause()
        {
            // don't listen to net changes out of the app - save some battery in bg mode
            if (netStateReceiver != null)
            {
                UnregisterReceiver(netStateReceiver);
                netStateReceiver.Dispose();
                netStateReceiver = null;
            }
            base.OnPause();
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Profile button click handler
        /// </summary>
        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(ProfileActivity));
            StartActivity(i);
        }

        /// <summary>
        /// History button click handler
        /// </summary>
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
                await RefreshData();
            }
            catch (AuthenticationException)
            {
                StartLoginActivity();
            }
            mainLayout.Refreshing = false;
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
                var result = await ensureRepo.AddLogAsync((EnsureTaste)tasteSpinner.SelectedItemPosition);
                if (result != null)
                {
                    Toast.MakeText(this, "Log Added", ToastLength.Long).Show();
                    todayProgress++;
                    UpdateProgressDiaplay();
                }
            }
            catch (AuthenticationException)
            {
                StartLoginActivity();
            }
            SetUiLoadingState(false);
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
                        await ensureRepo.SyncEnsures();
                        await RefreshData();
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
        #endregion

        #region Data
        /// <summary>
        /// Refreshes the Target and today's progress
        /// </summary>
        private async Task RefreshData()
        {
            SetUiLoadingState(true);
            // refresh today's logs (+history)
            await userService.RefreshInfo();
            var logs = await ensureRepo.GetLogs(DateTime.MinValue);
            todayProgress = logs.Count;
            UpdateProgressDiaplay();
            SetUiLoadingState(false); return;
        }

        /// <summary>
        /// Logs the user out and turns him to the login
        /// </summary>
        private async Task Logout()
        {
            SetUiLoadingState(true);
            StartLoginActivity(); // exit to login activity
            await userService.LogUserOut();
            ensures.Clear(); // Remove ensures & any other previous user data on UI
            mainProgressBar.Progress = 0;
            SetUiLoadingState(false);
        }
        #endregion

        #region Ui
        /// <summary>
        /// Changed the UI to match the loading state
        /// </summary>
        /// <param name="isLoading"></param>
        public void SetUiLoadingState(bool isLoading)
        {
            topLoadingProgress.Indeterminate = isLoading; // "disabled"
            mainLayout.Enabled = addBtn.Enabled = !isLoading;
        }


        /// <summary>
        /// Updates the target and the progress of today 
        /// </summary>
        private void UpdateProgressDiaplay()
        {
            var target = userService?.CurrentUser?.DailyTarget;
            if (!target.HasValue || target < 0) target = 0;
            todayProgressTv.Text = todayProgress + "/" + target.Value;
            mainProgressBar.Progress = todayProgress;
            mainProgressBar.Max = target.Value;
        }

        /// <summary>
        /// Starts the login activity
        /// </summary>
        private void StartLoginActivity()
        {
            var intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
        }
        #endregion

        #region Menu
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
                    _ = Logout();
                    break;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        #endregion

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}
