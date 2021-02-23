
using System;
using System.Collections.Generic;
using System.Linq;
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
    [Activity(Label = "Your History")]
    public class HistoryActivity : AppCompatActivity
    {
        // RecyclerView essentials
        private RecyclerView logsRv;
        private EnsureRecyclerAdapter ensuresRvAdapter;
        private EnsureLogTouchHelper logsRvTouchHelper;
        private List<InternalEnsureLog> ensures = new List<InternalEnsureLog>();

        private Button prevBtn, nextBtn;

        private DateTime displayedDate;

        private EnsureRepository ensureRepo;

        private TextView displayedDateTv, emptyListMessage;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_history);

            // Create your application here
            var mLayoutManager = new LinearLayoutManager(this);

            // Logs recycler view (almost the same as ListView, but you can swipe out items)
            logsRv = FindViewById<RecyclerView>(Resource.Id.EnsuresRv);
            logsRv.SetLayoutManager(mLayoutManager);
            // Data adapter
            ensuresRvAdapter = new EnsureRecyclerAdapter(ensures);
            logsRv.SetAdapter(ensuresRvAdapter);
            logsRvTouchHelper = new EnsureLogTouchHelper(this, ensuresRvAdapter);
            // Touch helper (to handle item swipes)
            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(logsRvTouchHelper);
            itemTouchHelper.AttachToRecyclerView(logsRv);

            prevBtn = FindViewById<Button>(Resource.Id.HistoryPrevBtn);
            prevBtn.Click += PrevBtn_Click;
            nextBtn = FindViewById<Button>(Resource.Id.HistoryNextBtn);
            nextBtn.Click += NextBtn_Click;

            displayedDateTv = FindViewById<TextView>(Resource.Id.DisplayedDateHistory);
            emptyListMessage = FindViewById<TextView>(Resource.Id.NothingMessageHistory);

            ensureRepo = new EnsureRepository(this);

            // show top back button
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            await UpdateDate(DateTime.Today);
        }

        private async void NextBtn_Click(object sender, EventArgs e)
        {
            await UpdateDate(displayedDate.AddDays(1)); // next day
        }

        private async void PrevBtn_Click(object sender, EventArgs e)
        {
            await UpdateDate(displayedDate.AddDays(-1)); // prev day
        }

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
            // show empty list message if it's gone
            emptyListMessage.Visibility = (
                logs.Count > 0 ? ViewStates.Gone : ViewStates.Visible);
            SetUiLoadingState(false);
        }

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


    private void SetUiLoadingState(bool isLoading) { }

        /// <summary> Ensure RecyclerView swipe/move event handler class </summary>
        private class EnsureLogTouchHelper : ItemTouchHelper.Callback
        {
            private readonly HistoryActivity context;
            private readonly EnsureRecyclerAdapter adapter;
            public bool EnableSwipe { get; set; }

            public EnsureLogTouchHelper(HistoryActivity context, EnsureRecyclerAdapter adapter)
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
                context.SetUiLoadingState(true);
                // Remove item from local dataset & server:
                var vh = (EnsureRecyclerAdapterViewHolder)viewHolder;
                // local
                adapter.Items.RemoveAll(l => l.Id == vh.LogId);
                adapter.NotifyDataSetChanged();
                // server
                HttpHelper helper = new HttpHelper(context);
                var res = await helper.PostAsync($"/api/Ensure/RemoveLog?id={vh.LogId}");
                if (!res.IsSuccessStatusCode)
                {
                }
                else
                {
                }
                context.SetUiLoadingState(false);
            }
        }
    }
}
