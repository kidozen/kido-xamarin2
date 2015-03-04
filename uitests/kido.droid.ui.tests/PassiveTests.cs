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
    class PassiveTests
    {
        static KidoApplication  kido = 
            new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);

        internal static void DoPassiveAuth(Context context)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback 
                += (sender, certificate, chain, sslPolicyErrors) => true;

            kido.Authenticate(context).
                ContinueWith(
                t =>
                {
                    var testResults = new Intent(context, typeof(TestsResultsActivity));
                    testResults.PutExtra("TestData", kido.CurrentUser.UserName);

                    context.StartActivity(testResults);
                }
            );
        }
    }
}