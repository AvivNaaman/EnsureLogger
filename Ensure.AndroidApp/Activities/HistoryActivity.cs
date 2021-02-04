
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Adapters;
using Ensure.AndroidApp.Data;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.Activities
{
    [Activity(Label = "HistoryActivity")]
    public class HistoryActivity : Activity
    {
        // RecyclerView essentials
        private RecyclerView logsRv;
        private EnsureRecyclerAdapter ensuresRvAdapter;
        private EnsureLogTouchHelper logsRvTouchHelper;
        private List<InternalEnsureLog> ensures = new List<InternalEnsureLog>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

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
