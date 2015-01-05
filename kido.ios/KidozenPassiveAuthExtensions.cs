using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.FSharp.Core;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using K = Kidozen;
using U = Utilities;
using A = KzApplication;

namespace Kidozen.iOS
{
	public static partial class KidozenExtensions
	{
		private static Task dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("task"));
		private static string authErrorMessage = "One or more errors occurred in Passive Authentication";
		private static Task dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});

		private static Kidozen.KidoApplication currentApplication;
		private static string curentConfiguration;
		private static A.AuthenticationRequest authenticationRequest;

		public static Task Authenticate(this Kidozen.KidoApplication app) {
			currentApplication = app;
			authenticationRequest = new A.AuthenticationRequest (app.marketplace, app.application, app.key, null);
			var appConfig = A.getAppConfig (A.createConfigUrl (app.marketplace, app.application));
			if (appConfig.IsConfiguration) {
				curentConfiguration = (appConfig as A.GetConfigurationResult.Configuration).Item;
				var signinurl = U.getJsonObjectValue (curentConfiguration , "signInUrl");
				if (signinurl.Value != null) {
					var authController = new PassiveAuthViewController (signinurl.Value.Replace("\"",string.Empty));
					authController.AuthenticationResponseArrived += HandleAuthenticationResponseArrived;
					var navController = new UINavigationController (authController);
					UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController (navController, 
						true, 
						new NSAction ( () => Debug.WriteLine("passive view loaded") )
					);
				} 
				else {
					throw new Exception ("Invalid configuration settings. Please check username and password");
				}
			}
			return Task.WhenAny( new Task[] {dummyPassiveAuthenticationTask, dummyPassiveFailTask} );
		}

		static void HandleAuthenticationResponseArrived (object sender, AuthenticationResponseEventArgs e)
		{
			Debug.WriteLine ("response from passive view arrived");
			if (e.Success) {
				if(dummyPassiveAuthenticationTask.Status==TaskStatus.RanToCompletion) dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("success passive auth"));

				var rawToken = e.TokenInfo ["access_token"];
				var refreshToken = e.TokenInfo ["refresh_token"];
				var token = new U.Token(new FSharpOption<string>(rawToken), new FSharpOption<string>(refreshToken),null);

				var expiration = A.getExpiration (rawToken);
				var identity = new KzApplication.Identity("3",rawToken, new FSharpOption<U.Token>(token) , curentConfiguration,expiration,authenticationRequest);
				currentApplication.setIdentity (identity);
				dummyPassiveAuthenticationTask.Start();
			}
			else {
				Debug.WriteLine (e.ErrorMessage);
				if(dummyPassiveFailTask.Status==TaskStatus.RanToCompletion)  
					dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});

				authErrorMessage = e.ErrorMessage;
				dummyPassiveFailTask.Start ();
			}
		}

	}
}

