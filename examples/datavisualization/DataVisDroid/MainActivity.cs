using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace DataVisDroid
{
    [Activity(Label = "DataVisDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Model kidoModel = new Model();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var authButton = FindViewById<Button>(Resource.Id.AuthenticateButton);
            var label = FindViewById<TextView>(Resource.Id.textView1);
            var invokeButton = FindViewById<Button>(Resource.Id.buttonInvoke);
            var editText = FindViewById<EditText>(Resource.Id.editTextDVName);

            authButton.Click += (sender, e) =>
            {
                kidoModel.Authenticate().ContinueWith(t =>
                RunOnUiThread(() =>
                {
                    label.Enabled = t.Result;
                    invokeButton.Enabled = t.Result;
                    editText.Enabled = t.Result;
                })
                );
            };

            invokeButton.Click += (sender, e) =>
            {
                RunOnUiThread(() => Toast.MakeText(this, "Launching visualization, please wait", ToastLength.Long).Show());

                kidoModel.DisplayDataVisualization(this, editText.Text)
                    .ContinueWith(t =>
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "Visualization finish, result: " + t.Result, ToastLength.Long).Show());
                    });
            };
        }
    }
}

