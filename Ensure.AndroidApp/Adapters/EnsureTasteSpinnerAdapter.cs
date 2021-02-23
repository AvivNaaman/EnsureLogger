using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ensure.Domain.Enums;

namespace Ensure.AndroidApp.Adapters
{
    public class EnsureTasteSpinnerAdapter : ArrayAdapter<EnsureTaste>
    {
        private Context context;

        public EnsureTasteSpinnerAdapter(Context context, List<EnsureTaste> enabledTastes)
            : base(context, Android.Resource.Layout.SimpleSpinnerItem, enabledTastes.ToArray())
        {
            EnabledTastes = enabledTastes;
            this.context = context;
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
                context.Resources.GetIdentifier(
                    "ensure_taste_" + taste.ToString().ToLower(), "drawable", context.PackageName
                    )
                );

            return baseItem;
        }

        public List<EnsureTaste> EnabledTastes { get; }
    }
}
