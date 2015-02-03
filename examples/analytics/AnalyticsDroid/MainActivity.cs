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
        private Model model = new Model();
        protected override void OnCreate(Bundle bundle)
        
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            var button = FindViewById<Button>(Resource.Id.MyButton);
            var buttonCustom = FindViewById<Button>(Resource.Id.buttonCustom);
            buttonCustom.Click += (o, eventArgs) => model.TagCustom(new { category = "bug" }); ;

            var buttonView = FindViewById<Button>(Resource.Id.buttonView);
            buttonView.Click += (sender, args) => model.TagView(this.GetType().FullName);

            var buttonTag = FindViewById<Button>(Resource.Id.buttonTag);
            buttonTag.Click += (sender, args) => model.TagButton("button clicked at: " + DateTime.Now.ToString());

            button.Click += (sender, args) =>
            {
                model.Authenticate().ContinueWith(auth =>
                {
                    if (auth.Result)
                    {
                        buttonCustom.Enabled = true;
                        buttonView.Enabled = true;
                        buttonTag.Enabled = true;

                        model.EnableAnalytics(this.Application);
                    }
                });

                //StartActivity(typeof (Activity1));
            };
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            
        }
    }
}

