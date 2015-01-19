using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using U = Utilities;
using A = KzApplication;

using Android.Content;

namespace Kidozen.Android
{
	public static partial class KidozenExtensions
	{
		// Allows to expose Authneticate method as a Task.
		private static Task dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("task"));
		private static string authErrorMessage = "One or more errors occurred in Passive Authentication";
		private static Task dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});

		private static Kidozen.KidoApplication currentApplication;
		
		public static Task Authenticate(this Kidozen.KidoApplication app, Context context) {
			currentApplication = app;
            var url = A.fetchConfigValue("signInUrl", app.marketplace, app.application, app.key);
            var startPassiveAuthIntent = new Intent(context, typeof(PassiveAuthActivity));
            startPassiveAuthIntent.AddFlags(ActivityFlags.NewTask);
            startPassiveAuthIntent.PutExtra("signInUrl", url.Result.Replace("\"", string.Empty));
            context.StartActivity(startPassiveAuthIntent);
            return Task.WhenAny( new Task[] {dummyPassiveAuthenticationTask, dummyPassiveFailTask} );
		}

		public static void HandleAuthenticationResponseArrived (AuthenticationResponseEventArgs e)
		{
			Debug.WriteLine ("response from passive view arrived");
			resetTaks ();
			if (e.Success) {
				var rawToken = e.TokenInfo ["access_token"];
				var refreshToken = e.TokenInfo ["refresh_token"];
                currentApplication.setPassiveIdentity(rawToken, refreshToken, currentApplication.GetCurrentConfiguragion);
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

