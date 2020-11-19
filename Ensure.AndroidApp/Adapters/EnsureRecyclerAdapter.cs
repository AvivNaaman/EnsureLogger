using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Ensure.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ensure.AndroidApp.Adapters
{
	public class EnsureRecyclerAdapter : RecyclerView.Adapter
	{
		public List<EnsureLog> Items { get; set; }

		public event EventHandler<EnsureRecyclerAdapterClickEventArgs> ItemClick;
		public event EventHandler<EnsureRecyclerAdapterClickEventArgs> ItemLongClick;

		public EnsureRecyclerAdapter(List<EnsureLog> data)
		{
			Items = data;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{

			var id = Resource.Layout.ensure_recycler_item_layout;
			var itemView = LayoutInflater.From(parent.Context).
			       Inflate(id, parent, false);

			var vh = new EnsureRecyclerAdapterViewHolder(itemView, OnClick, OnLongClick);
			return vh;
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			var item = Items[position];
			// Replace the contents of the view with that element
			var holder = viewHolder as EnsureRecyclerAdapterViewHolder;
			holder.DateLoggedTv.Text = item.Logged.ToShortTimeString();
			holder.TasteTv.Text = item.EnsureTaste.ToString();
			holder.LogId = item.Id;
		}

		public override int ItemCount => Items.Count;

		void OnClick(EnsureRecyclerAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(EnsureRecyclerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

	}

	public class EnsureRecyclerAdapterViewHolder : RecyclerView.ViewHolder
	{
		//public TextView TextView { get; set; }
		public TextView DateLoggedTv { get; set; }
		public TextView TasteTv { get; set; }
		public string LogId { get; set; }

		public EnsureRecyclerAdapterViewHolder(View itemView, Action<EnsureRecyclerAdapterClickEventArgs> clickListener,
							Action<EnsureRecyclerAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			DateLoggedTv = ItemView.FindViewById<TextView>(Resource.Id.EnsureRvAdapterDate);
			TasteTv = ItemView.FindViewById<TextView>(Resource.Id.EnsureRvAdapterTaste);
			itemView.Click += (sender, e) => clickListener(new EnsureRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
			itemView.LongClick += (sender, e) => longClickListener(new EnsureRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
		}
	}

	public class EnsureRecyclerAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}