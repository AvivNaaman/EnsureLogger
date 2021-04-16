
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Adapters;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp
{
    [Activity(Label = "Manage History")]
    public class HistoryActivity : AppCompatActivity, ILoadingStatedActivity
    {
        // RecyclerView essentials
        private RecyclerView logsRv;
        private EnsureRecyclerAdapter ensuresRvAdapter;
        private EnsureLogTouchHelper logsRvTouchHelper;
        private List<InternalEnsureLog> ensures = new List<InternalEnsureLog>();

        private Button prevBtn, nextBtn;

        private DateTime displayedDate;

        private TextView displayedDateTv, emptyListMessage;

        private ProgressBar topLoadingProgress;

        private EnsureRepository ensureRepo;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_history);

            ensureRepo = new EnsureRepository(this);

            #region RvSetup
            // Create your application here
            var mLayoutManager = new LinearLayoutManager(this);

            // Logs recycler view (almost the same as ListView, but you can swipe out items)
            logsRv = FindViewById<RecyclerView>(Resource.Id.EnsuresRv);
            logsRv.SetLayoutManager(mLayoutManager);
            // Data adapter
            ensuresRvAdapter = new EnsureRecyclerAdapter(ensures);
            logsRv.SetAdapter(ensuresRvAdapter);
            logsRvTouchHelper = new EnsureLogTouchHelper();
            logsRvTouchHelper.OnItemRecycled += LogsRv_ItemSwiped;
            // Touch helper (to handle item swipes)
            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(logsRvTouchHelper);
            itemTouchHelper.AttachToRecyclerView(logsRv);
            #endregion

            prevBtn = FindViewById<Button>(Resource.Id.HistoryPrevBtn);
            prevBtn.Click += PrevBtn_Click;
            nextBtn = FindViewById<Button>(Resource.Id.HistoryNextBtn);
            nextBtn.Click += NextBtn_Click;

            displayedDateTv = FindViewById<TextView>(Resource.Id.DisplayedDateHistory);
            emptyListMessage = FindViewById<TextView>(Resource.Id.NothingMessageHistory);
            topLoadingProgress = FindViewById<ProgressBar>(Resource.Id.HistoryActivityTopProgress);

            ensureRepo = new EnsureRepository(this);

            // show top back button
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            await UpdateDate(DateTime.Today);
        }

        #region Handlers
        /// <summary>
        /// Handler for next day button click
        /// </summary>
        private async void NextBtn_Click(object sender, EventArgs e)
        {
            await UpdateDate(displayedDate.AddDays(1)); // next day
            nextBtn.Enabled = displayedDate.AddDays(1) <= DateTime.Now;
        }

        /// <summary>
        /// Handler for previous day button click
        /// </summary>
        private async void PrevBtn_Click(object sender, EventArgs e)
        {
            await UpdateDate(displayedDate.AddDays(-1)); // prev day
            nextBtn.Enabled = true;
        }

        /// <summary>
        /// Handler for item swipe
        /// </summary>
        /// <param name="viewHolder"></param>
        private async void LogsRv_ItemSwiped(RecyclerView.ViewHolder viewHolder)
        {

            SetUiLoadingState(true);
            // Remove item from local dataset & server:
            var vh = (EnsureRecyclerAdapterViewHolder)viewHolder;
            // disaplay
            ensuresRvAdapter.Items.RemoveAll(l => l.Id == vh.LogId);
            ensuresRvAdapter.NotifyDataSetChanged();
            // data
            await ensureRepo.RemoveLogAsync(vh.LogId);
            SetUiLoadingState(false);
        }
        #endregion

        #region Data
        private async Task UpdateDate(DateTime newDate)
        {
            // update value & UI, then call update list
            displayedDate = newDate;
            displayedDateTv.Text = newDate.ToString("dd/MM/yyyy");
            await UpdateList();
        }
        private async Task UpdateList()
        {
            // fetch & put in the list
            SetUiLoadingState(true);
            var logs = await ensureRepo.GetLogs(displayedDate);
            ensures.Clear();
            ensures.AddRange(logs);
            // notify change to the UI
            ensuresRvAdapter.NotifyDataSetChanged();
            // show empty list message if no logs for the date
            emptyListMessage.Visibility = (
                logs.Count > 0 ? ViewStates.Gone : ViewStates.Visible);
            SetUiLoadingState(false);
        }
        #endregion

        /// <summary>
        /// Handler for home button click
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }


        public void SetUiLoadingState(bool isLoading)
        {
            topLoadingProgress.Indeterminate = isLoading;
            logsRvTouchHelper.EnableSwipe = prevBtn.Enabled = !isLoading;
            nextBtn.Enabled = !isLoading && displayedDate.AddDays(1) <= DateTime.Now;
        }

        /// <summary> Ensure RecyclerView swipe/move event handler class </summary>
        private class EnsureLogTouchHelper : ItemTouchHelper.Callback
        {
            public bool EnableSwipe { get; set; }

            public delegate void ItemRecycledEvent(RecyclerView.ViewHolder viewHolder);

            public event ItemRecycledEvent OnItemRecycled;

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

            public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
            {
                OnItemRecycled?.Invoke(viewHolder); // invoke event if has subscribers
            }
        }
    }
}
