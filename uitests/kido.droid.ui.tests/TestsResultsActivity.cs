using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace kido.droid.ui.tests
{
    [Activity(Label = "TestsResultsActivity")]
    public class TestsResultsActivity : Activity
    {
        TextView tv;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.TestsResultsActivity);

            string text = Intent.GetStringExtra("TestData") ?? "Data not available";

            tv = FindViewById<TextView>(Resource.Id.textViewResults);
            tv.Text = text;

        }
    }
}