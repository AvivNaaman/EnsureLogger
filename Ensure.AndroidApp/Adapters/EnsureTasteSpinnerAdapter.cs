using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ensure.Domain.Enums;

namespace Ensure.AndroidApp.Adapters
{
    /// <summary>
    /// Simple ArrayAdapter to case the ensure tastes for Spinner items, including
    /// the image of the ensure.
    /// </summary>
    public class EnsureTasteSpinnerAdapter : ArrayAdapter<EnsureTaste>
    {

        public EnsureTasteSpinnerAdapter(Context context, List<EnsureTaste> enabledTastes)
            : base(context, Android.Resource.Layout.SimpleSpinnerItem, enabledTastes.ToArray())
        {
            EnabledTastes = enabledTastes;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return GetItemView(EnabledTastes[position], parent);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetItemView(EnabledTastes[position], parent);
        }

        public View GetItemView(EnsureTaste taste, ViewGroup parent)
        {
            var baseItem = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.ensure_taste_spinner_item, parent, false);

            // set layout text & image
            baseItem.FindViewById<TextView>(Resource.Id.TasteSpinnerItemTv).Text = taste.ToString();
            baseItem.FindViewById<ImageView>(Resource.Id.TasteSpinnerItemIv)
                .SetImageResource(
                parent.Context.Resources.GetIdentifier(
                    "ensure_taste_" + taste.ToString().ToLower(), "drawable", parent.Context.PackageName
                    )
                );

            return baseItem;
        }

        public List<EnsureTaste> EnabledTastes { get; }
    }
}
