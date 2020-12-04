﻿using Android.App;
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

namespace Ensure.AndroidApp
{
	[Activity(Label = "@string/app_name", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private List<EnsureLog> ensures = new List<EnsureLog>();

		private EnsureRecyclerAdapter ensuresRvAdapter;
		private EnsureLogTouchHelper logsRvTouchHelper;

		private ProgressBar horizontalTopProgress;
		private Spinner tasteSpinner;
		private Button addBtn;
		private RecyclerView logsRv;
		private SwipeRefreshLayout refreshLayout;

		private TextView helloUserTv;

		private TextView currDayTv;
		private Button prevDayBtn;
		private Button nextDayBtn;

		private ProgressBar todayProgress;

		private DateTime currDisplayedDate = DateTime.MinValue;
		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			// Move to login activity if hsn't logged in yet
			var app = (EnsureApplication)ApplicationContext;

			if (!app.IsLoggedIn)
			{
				StartLoginActivity();
			}

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			// Hello, UserName message
			helloUserTv = FindViewById<TextView>(Resource.Id.HelloUserTv);

			// Taste picker
			tasteSpinner = FindViewById<Spinner>(Resource.Id.EnsureTasteSpinner);
			var tastesList = Enum.GetValues(typeof(EnsureTaste)).Cast<EnsureTaste>().ToList();
			tasteSpinner.Adapter = new ArrayAdapter<EnsureTaste>(this, Android.Resource.Layout.SimpleListItem1, tastesList);

			// Add button
			addBtn = FindViewById<Button>(Resource.Id.addBtn);
			addBtn.Click += AddBtn_Click;

			// Top progress bar
			horizontalTopProgress = FindViewById<ProgressBar>(Resource.Id.MainActivityTopProgress);

			// Swipe to refresh layout
			refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.MainActivityRefreshableLayout);
			refreshLayout.Refresh += MainActivity_Refresh;

			currDayTv = FindViewById<TextView>(Resource.Id.CurrDayTv);
			prevDayBtn = FindViewById<Button>(Resource.Id.PrevDayBtn);
			prevDayBtn.Click += async (sender, e) => await ChangeDateBtn_Clicked(sender, e, -1);
			nextDayBtn = FindViewById<Button>(Resource.Id.NextDayBtn);
			nextDayBtn.Click += async (sender, e) => await ChangeDateBtn_Clicked(sender, e, 1);

			var mLayoutManager = new LinearLayoutManager(this);
			// Logs recycler view (almost the same as ListView, but you can swipe out items)
			logsRv = FindViewById<RecyclerView>(Resource.Id.EnsuresRv);
			logsRv.SetLayoutManager(mLayoutManager);
			ensuresRvAdapter = new EnsureRecyclerAdapter(ensures);
			logsRv.SetAdapter(ensuresRvAdapter);
			logsRvTouchHelper = new EnsureLogTouchHelper(this, ensuresRvAdapter);
			ItemTouchHelper itemTouchHelper = new ItemTouchHelper(logsRvTouchHelper);
			itemTouchHelper.AttachToRecyclerView(logsRv);

			todayProgress = FindViewById<ProgressBar>(Resource.Id.TodayProgressBar);

			if (app.IsLoggedIn)
			{
				helloUserTv.Text = $"Hello, {app.UserInfo.UserName}";
				await RefreshUserTargetStatus();
				await RefreshEnsuresList();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		private async void MainActivity_Refresh(object sender, EventArgs e)
		{
			const string MainActivityRefreshTag = "MainActivity.Refresh";
			Log.Debug(MainActivityRefreshTag, "Refresh called");
			await RefreshEnsuresList();
			Log.Debug(MainActivityRefreshTag, "Refresh done");
			refreshLayout.Refreshing = false;
		}

		private async void AddBtn_Click(object sender, EventArgs e)
		{
			const string MainActivityAddBtnClickTag = "MainActivity.AddBtnClick";
			try
			{
				// TODO: Call /api/Ensures/AddLog?taste={EnsureTaste}
				SetUiLoadingState(true);
				var http = new HttpHelper(this);
				int tasteVal = tasteSpinner.SelectedItemPosition;
				Log.Debug(MainActivityAddBtnClickTag, $"Adding log with {tasteVal} taste.");
				var res = await http.PostAsync($"/api/Ensure/AddLog?taste={tasteVal}");
				if (!res.IsSuccessStatusCode)
				{
					Log.Debug(MainActivityAddBtnClickTag, $"Adding log failed with status code {res.StatusCode}");
					if (res.StatusCode.HasFlag(System.Net.HttpStatusCode.Unauthorized) || res.StatusCode.HasFlag(System.Net.HttpStatusCode.Forbidden))
					{
						StartLoginActivity();
						return;
					}
					else
					{
						// TODO: Decide how to put an error to the users' faces} #2
					}
				}
				var result = JsonConvert.DeserializeObject<EnsureLog>(await res.Content.ReadAsStringAsync());
				if (result != null)
				{
					Log.Debug(MainActivityAddBtnClickTag, $"Add succeeded! new log's Id is {result.Id}");
					ensures.Insert(0, result);
					ensuresRvAdapter.NotifyDataSetChanged();
					Toast.MakeText(this, "Log Added", ToastLength.Long).Show();
					todayProgress.Progress++;
				}
			}
			catch
			{

			}
			SetUiLoadingState(false);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			ActivityRequestCodes code = (ActivityRequestCodes)requestCode;
			var app = (EnsureApplication)ApplicationContext;
			switch (code)
			{
				case ActivityRequestCodes.Login: // login
					helloUserTv.Text = $"Hello, {app.UserInfo.UserName}";
					todayProgress.Max = app.UserInfo.DailyTarget;
					await RefreshEnsuresList();
					break;
				default:
					break;
			}
		}

		private async Task RefreshUserTargetStatus()
		{
			SetUiLoadingState(true);
			var http = new HttpHelper(this);
			var res = await http.GetAsync("/api/Account/GetTarget");
			if (!res.IsSuccessStatusCode)
			{
				if (res.StatusCode.HasFlag(System.Net.HttpStatusCode.Unauthorized) || res.StatusCode.HasFlag(System.Net.HttpStatusCode.Forbidden))
				{
					Log.Info("RefreshTarget", $"Authentication is required by status code {res.StatusCode}");
					StartLoginActivity();
					SetUiLoadingState(false);
					return;
				}
				else
				{
					// TODO: Decide how to put an error to the users' faces
				}
			}

			try
			{
				string re = await res.Content.ReadAsStringAsync();
				short target = short.Parse(re);
				((EnsureApplication)ApplicationContext).UserInfo.DailyTarget = target;
				todayProgress.Max = target;
			}
			catch
			{

			}

			SetUiLoadingState(false);
		}

		private async Task RefreshEnsuresList()
		{
			const string EnsuresRefreshTag = "RefreshEnsure";
			SetUiLoadingState(true);
			var http = new HttpHelper(this);
			Log.Debug(EnsuresRefreshTag, "Fetch started");
			string dateStr = currDisplayedDate > DateTime.MinValue ? currDisplayedDate.ToString(EnsureConstants.DateTimeUrlFormat) : String.Empty;
			var res = await http.GetAsync($"/api/Ensure/GetLogs?date={dateStr}");
			Log.Debug(EnsuresRefreshTag, "Fetch done");

			if (!res.IsSuccessStatusCode)
			{
				if (res.StatusCode.HasFlag(System.Net.HttpStatusCode.Unauthorized) || res.StatusCode.HasFlag(System.Net.HttpStatusCode.Forbidden))
				{
					Log.Info(EnsuresRefreshTag, $"Authentication is required by status code {res.StatusCode}");
					StartLoginActivity();
					SetUiLoadingState(false);
					return;
				}
				else
				{
					// TODO: Decide how to put an error to the users' faces
				}
			}

			try
			{
				// TODO: Check for a better way to handle this...
				Log.Debug(EnsuresRefreshTag, "Deserializing result");
				var newEnsures = JsonConvert.DeserializeObject<ApiEnsuresList>(await res.Content.ReadAsStringAsync());
				if (currDisplayedDate <= DateTime.MinValue)
				{
					currDisplayedDate = newEnsures.CurrentReturnedDate;
				}
				currDayTv.Text = currDisplayedDate.ToShortDateString();
				ensures = newEnsures.Logs;
				ensuresRvAdapter.Items = ensures;
				ensuresRvAdapter.NotifyDataSetChanged();
				// TODO: today progress
			}
			catch
			{

			}
			res = await http.GetAsync("/api/Ensure/TodayProgress");
			if (!res.IsSuccessStatusCode)
			{

			}


			try
			{
				short progress = short.Parse(await res.Content.ReadAsStringAsync());
				todayProgress.Progress = progress;
			}
			catch
			{

			}

			SetUiLoadingState(false);
		}

		private void StartLoginActivity()
		{
			const string ActivityMainStartLoginTag = "ActivityMain.StartLogin";
			Log.Debug(ActivityMainStartLoginTag, $"Starting Login Activity");
			var intent = new Intent(this, typeof(LoginActivity));
			StartActivityForResult(intent, (int)ActivityRequestCodes.Login);
		}

		private void SetUiLoadingState(bool isLoading)
		{

			const string ActivityMainSetUiLoadingStateTag = "ActivityMain.SetUiLoadingState";
			Log.Debug(ActivityMainSetUiLoadingStateTag, $"Settings UI Loading State: {isLoading}");
			horizontalTopProgress.Indeterminate = isLoading; // "disabled"
			refreshLayout.Enabled = nextDayBtn.Enabled = prevDayBtn.Enabled =
				addBtn.Enabled = logsRv.Enabled = logsRvTouchHelper.EnableSwipe = !isLoading;
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.main_menu, menu);

			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			SetUiLoadingState(true);
			StartLoginActivity(); // exit to login activity
			((EnsureApplication)ApplicationContext).LogUserOut();
			ensures.Clear(); // Remove ensures & any other previous user data on UI
			ensuresRvAdapter.NotifyDataSetChanged();
			todayProgress.Progress = 0;
			SetUiLoadingState(false);
			return base.OnOptionsItemSelected(item);
		}

		private async Task ChangeDateBtn_Clicked(object sender, EventArgs e, short DaysToAdd)
		{
			// add DaysToAdd to current date and then
			currDisplayedDate = DaysToAdd > 0 ? currDisplayedDate.AddDays(DaysToAdd) : currDisplayedDate.Subtract(TimeSpan.FromDays(-DaysToAdd));
			// refrsh ensures by new date
			await RefreshEnsuresList();
		}

		enum ActivityRequestCodes
		{
			Login
		}

		/// <summary> Ensure RecyclerView swipe/move event handler class </summary>
		public class EnsureLogTouchHelper : ItemTouchHelper.Callback
		{
			private readonly MainActivity context;
			private readonly EnsureRecyclerAdapter adapter;
			public bool EnableSwipe { get; set; }
			public EnsureLogTouchHelper(MainActivity context, EnsureRecyclerAdapter adapter)
			{
				this.context = context;
				this.adapter = adapter;
			}
			public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
			{
				int dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down;
				int swipeFlags = EnableSwipe ? ItemTouchHelper.Start | ItemTouchHelper.End : 0;
				return MakeMovementFlags(dragFlags, swipeFlags);
			}

			public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
			{
				return true;
			}

			public override async void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
			{
				const string EnsureTouchHelperOnSwipedTag = "EnsureTouchHelper.OnSwipe";
				context.SetUiLoadingState(true);
				Log.Debug(EnsureTouchHelperOnSwipedTag, $"Log with id {viewHolder.ItemId} swiped. Removing..");
				// Remove item from local dataset & server:
				var vh = (EnsureRecyclerAdapterViewHolder)viewHolder;
				// local
				adapter.Items.RemoveAll(l => l.Id == vh.LogId);
				Log.Debug(EnsureTouchHelperOnSwipedTag, $"Removed from local");
				adapter.NotifyDataSetChanged();
				// server
				HttpHelper helper = new HttpHelper(context);
				Log.Debug(EnsureTouchHelperOnSwipedTag, $"Removing from remote");
				var res = await helper.PostAsync($"/api/Ensure/RemoveLog?id={vh.LogId}");
				if (!res.IsSuccessStatusCode)
				{
					Log.Error(EnsureTouchHelperOnSwipedTag, $"Remove item {viewHolder.ItemId} from remote failed");
				}
				else
				{
					context.todayProgress.Progress--;
					Log.Debug(EnsureTouchHelperOnSwipedTag, "Removing from remote succeeded.");
				}
				context.SetUiLoadingState(false);
			}
		}

	}
}
