using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Kidozen;
using Kidozen.Android;

namespace crashdroid
{
    [Activity(Label = "crashdroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        public KidoApplication kido = new KidoApplication("tenant.kidocloud.com", "tasks", "appkey");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            kido.EnableCrash(this.BaseContext);
            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += (o, s) => { throw new NullReferenceException(); };
        }
    }
}

