using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using U = Utilities;
using A = KzApplication;
using WebSocket4Net;
using Android.Webkit;

namespace Kidozen.Android
{
	public static partial class KidozenExtensions
	{
		// Allows to expose Authneticate method as a Task.
		private static Task dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("task"));
		private static string authErrorMessage = "One or more errors occurred in Passive Authentication";
		private static Task dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});

		private static KidoApplication currentApplication;
        private static Context currentAndroidContext;
    
		public static Task Authenticate(this KidoApplication app, Context context) {
            if (dummyPassiveAuthenticationTask.Status == TaskStatus.RanToCompletion)
            {
                dummyPassiveAuthenticationTask = new Task(() => Console.WriteLine("task"));
            }
            if (dummyPassiveFailTask.Status == TaskStatus.RanToCompletion)
            {
                dummyPassiveFailTask = new Task(() => { throw new Exception(authErrorMessage); });
            }
			currentApplication = app;
            currentAndroidContext = context;

            var url = A.fetchConfigValue("signInUrl", app.marketplace, app.application, app.key);
            var startPassiveAuthIntent = new Intent(context, typeof(PassiveAuthActivity));
            startPassiveAuthIntent.AddFlags(ActivityFlags.NewTask);
            startPassiveAuthIntent.PutExtra("signInUrl", url.Result.Replace("\"", string.Empty));
            context.StartActivity(startPassiveAuthIntent);
            return Task.WhenAny( new Task[] {dummyPassiveAuthenticationTask, dummyPassiveFailTask} );
		}

        public static void SignOut(this Kidozen.KidoApplication app)
        {
            try
            {
                Debug.WriteLine("SignOut extension is being called");
                CookieSyncManager.CreateInstance(currentAndroidContext);
                CookieManager cookieManager = CookieManager.Instance;
                cookieManager.RemoveAllCookie();
                currentApplication.logOut(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not clear cookies for passive authentication. Please restart the application and try again");                //throw;
            }
        }

		public static void HandleAuthenticationResponseArrived (AuthenticationResponseEventArgs e)
		{
			Debug.WriteLine ("response from passive view arrived");
			resetTaks ();
			if (e.Success) {
				var rawToken = e.TokenInfo ["access_token"];
				var refreshToken = e.TokenInfo ["refresh_token"];
                currentApplication.setPassiveIdentity(rawToken, refreshToken);
            	dummyPassiveAuthenticationTask.Start ();
			}
			else {
				Debug.WriteLine (e.ErrorMessage);
				authErrorMessage = e.ErrorMessage;
				dummyPassiveFailTask.Start ();
			}
		}

		static void resetTaks () {
			if(dummyPassiveAuthenticationTask.Status==TaskStatus.RanToCompletion) 
				dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("success passive auth"));
			if(dummyPassiveFailTask.Status==TaskStatus.RanToCompletion)  
				dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});
		}

    }
}

