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

using Kidozen;
using Kidozen.Android;

namespace kido.droid.ui.tests
{
    public class PSMessage
    {
        public string bar { get; set; }
    }

    [Activity(Label = "TestsResultsActivity")]
    public class TestsResultsActivity : Activity
    {
        TextView tv;
        Kidozen.PubSub ps;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.TestsResultsActivity);

            string text = Intent.GetStringExtra("TestData") ?? "Data not available";

            var btn = FindViewById<TextView>(Resource.Id.button1);
            btn.Click += (s,o) => {
                ps.Publish<PSMessage>(new PSMessage { bar = Guid.NewGuid().ToString() });
            };
            var button = FindViewById<Button>(Resource.Id.button1);
            var kidozen = new Kidozen.KidoApplication("kidodemo.kidocloud.com"
                , "tasks"
                , "wb8KTX2/21A6ISM7PncaNozhxxCxcL8+TtB2aKbZyu8=");
            kidozen.Authenticate("demo@kidozen.com", "pass", "Kidozen").ContinueWith(authTask =>
            {
                try
                {
                    ps = kidozen.SubscribeToChannel<PSMessage>("ABCDEF-000011", onMessageArrive);
                    var ok = ps.Subscribe().ContinueWith(
                        t =>
                        {
                            Console.WriteLine("Task Subscribe Result: " + t.Result.ToString());
                        }
                    );
                }
                catch (Exception e)
                {
                    throw;
                }
                //
                //InvokeOnMainThread(()=> {
                //assert
                //var expected = testExpectedDetail as KidoUIXTestDetails<string>;
                //var isOk = expected.ExpectedValue == kidozen.CurrentUser.UserName;
                //detailDescriptionLabel.Text = kidozen.CurrentUser.UserName;
                //    })
            });
        }

        private void onMessageArrive(object obj, EventArgs value)
        {
            Console.WriteLine("Message arrived: " + value.ToString());
        }


    }
}