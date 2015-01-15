using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Kidozen.Examples.Portable;
namespace AndroidDs
{
    [Activity(Label = "AndroidDs", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += (s, e) => {
                var ds = new MyDataSource();
                ds.Authenticate().ContinueWith(
                    t =>
                    {
                        var isauth = t.Result;
                        if (isauth)
                        {
                            var result = ds.QueryDataSoruce<DsParams>("getTellagoVacations", new DsParams { qty = 2 }).Result;
                            System.Diagnostics.Debug.WriteLine(result.ToString());

                        }
                    }
                    );

            };
        }
    }

    class DsParams
    {
        public int qty { get; set; }
    }
}

