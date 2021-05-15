using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Ensure.AndroidApp.Data;
using Ensure.Domain.Entities;
using Ensure.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Ensure.AndroidApp.Adapters
{
	/// <summary>
    /// A RecyclerView's Adapter class for matching Logged ensures and their views inside the RecyclerView.
    /// </summary>
	public class EnsureRecyclerAdapter : RecyclerView.Adapter
	{
		/// <summary>
        /// The currently displayed data
        /// </summary>
		public List<InternalEnsureLog> Items { get; set; }

		// handlers for events
		public event EventHandler<EnsureRecyclerAdapterClickEventArgs> ItemClick;
		public event EventHandler<EnsureRecyclerAdapterClickEventArgs> ItemLongClick;

		public EnsureRecyclerAdapter(List<InternalEnsureLog> data)
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
			holder.LogId = item.Id;
			holder.SetTaste(item.EnsureTaste);
				
		}

		public override int ItemCount => Items.Count;

		// I won't use these supllied events BUT there will be TouchHelper for swiping items out event handling.
		void OnClick(EnsureRecyclerAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(EnsureRecyclerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

	}

	/// <summary>
    /// A ViewHolder for the RecyclerView items.
    /// </summary>
	public class EnsureRecyclerAdapterViewHolder : RecyclerView.ViewHolder
	{
		public TextView DateLoggedTv { get; set; }
		public TextView TasteTv { get; set; }
		public string LogId { get; set; }
		public ImageView TasteImage { get; set; }

		public EnsureRecyclerAdapterViewHolder(View itemView, Action<EnsureRecyclerAdapterClickEventArgs> clickListener,
							Action<EnsureRecyclerAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			DateLoggedTv = ItemView.FindViewById<TextView>(Resource.Id.EnsureRvAdapterDate);
			TasteTv = ItemView.FindViewById<TextView>(Resource.Id.EnsureRvAdapterTaste);
			itemView.Click += (sender, e) => clickListener(new EnsureRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
			itemView.LongClick += (sender, e) => longClickListener(new EnsureRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
			// set image
			TasteImage = ItemView.FindViewById<ImageView>(Resource.Id.EnsureReceyclerItemIv);
		}

		/// <summary>
        /// Set The taste of the currently disaplyed ensure, updating it's UI to match the new taste.
        /// </summary>
        /// <param name="taste"></param>
		public void SetTaste(EnsureTaste taste)
        {
			TasteImage.SetImageResource(TasteImage.Context.Resources.GetIdentifier("ensure_taste_" +
					taste.ToString().ToLower(), "drawable", TasteImage.Context.PackageName));
			TasteTv.Text = taste.ToString();
		}
	}

	public class EnsureRecyclerAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}