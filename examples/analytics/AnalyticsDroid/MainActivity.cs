using System;
using Android;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using  Kidozen;
using Kidozen.Android;

namespace AnalyticsDroid
{
    [Activity(Label = "AnalyticsDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private KidoApplication kido = null;
        protected override void OnCreate(Bundle bundle)
        
        {
            kido = new KidoApplication("demo.kidocloud.com", "testxs", "m/H5esSrQKfFpbtFl4Qtn6j4EVg4UbpTnY6tOwcfb70=");
  
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            var button = FindViewById<Button>(Resource.Id.MyButton);
            

            
            button.Click += (sender, args) =>
            {

                StartActivity(typeof (Activity1));
            };
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            kido.EnableAnalytics(this.Application);
        }
    }
}

