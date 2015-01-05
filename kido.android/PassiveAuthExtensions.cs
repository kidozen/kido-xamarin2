using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.FSharp.Core;

using K = Kidozen;
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
		private static string curentConfiguration;
		private static A.AuthenticationRequest authenticationRequest;

		public static Task Authenticate(this Kidozen.KidoApplication app, Context context) {
			currentApplication = app;
			authenticationRequest = new A.AuthenticationRequest (app.marketplace, app.application, app.key, null);
			var appConfig = A.getAppConfig (A.createConfigUrl (app.marketplace, app.application));
			resetTaks ();
			if (appConfig.IsConfiguration) {
				curentConfiguration = (appConfig as A.GetConfigurationResult.Configuration).Item;

				var signinurl = U.getJsonObjectValue (curentConfiguration , "signInUrl");

				if (signinurl.Value != null) {
					var startPassiveAuthIntent = new Intent (context, typeof(PassiveAuthActivity));
					startPassiveAuthIntent.AddFlags (ActivityFlags.NewTask);
					startPassiveAuthIntent.PutExtra ("signInUrl", signinurl.Value.Replace("\"",string.Empty));
					context.StartActivity (startPassiveAuthIntent);
				} 
				else {
					throw new Exception ("Invalid configuration settings. Please check username and password");
				}
			}
			return Task.WhenAny( new Task[] {dummyPassiveAuthenticationTask, dummyPassiveFailTask} );
		}

		public static void HandleAuthenticationResponseArrived (AuthenticationResponseEventArgs e)
		{
			Debug.WriteLine ("response from passive view arrived");
			resetTaks ();
			if (e.Success) {
				var rawToken = e.TokenInfo ["access_token"];
				var refreshToken = e.TokenInfo ["refresh_token"];
				var token = new U.Token(new FSharpOption<string>(rawToken), new FSharpOption<string>(refreshToken),null);

				var expiration = A.getExpiration (rawToken);
				var identity = new KzApplication.Identity("3",rawToken, new FSharpOption<U.Token>(token) , curentConfiguration,expiration,authenticationRequest);

				currentApplication.setIdentity (identity);
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

