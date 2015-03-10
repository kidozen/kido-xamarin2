using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Kidozen;
using Kidozen.Android;

namespace kido.droid.ui.tests
{
    [Activity(Label = "kido.droid.ui.tests", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : ListActivity
    {
        string[] testList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            testList = new string[] { "passiveAuthentication"};

            SetContentView(Resource.Layout.Main);
            ListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, testList);
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var t = testList[position];
            //PassiveTests.DoPassiveAuth(this);
            var p = new PassiveTests();
            p.testPubSub(this);
        }
    }
}

