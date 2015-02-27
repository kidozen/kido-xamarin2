using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

#if __UNIFIED__
using MonoTouch;
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

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
		
		public static Task Authenticate(this Kidozen.KidoApplication app) {
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                currentApplication = app;
                var url = A.fetchConfigValue("signInUrl", app.marketplace, app.application, app.key);
                var authController = new PassiveAuthViewController(url.Result.Replace("\"", string.Empty));
                authController.AuthenticationResponseArrived += HandleAuthenticationResponseArrived;
                var navController = new UINavigationController(authController);
#if __UNIFIED__
                UIApplication.SharedApplication.Delegate.GetWindow().RootViewController.PresentViewController(navController,
                    true,
                    new Action(() => Debug.WriteLine("passive view loaded"))
                );
#else
        UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController(navController,
            true,
            new NSAction(() => Debug.WriteLine("passive view loaded"))
        );
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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
                currentApplication.setPassiveIdentity(rawToken, refreshToken);

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

