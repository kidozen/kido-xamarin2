using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using U = Utilities;
using A = KzApplication;
using WebSocket4Net;

namespace Kidozen.Android
{
	public static partial class KidozenExtensions
	{
		// Allows to expose Authneticate method as a Task.
		private static Task dummyPassiveAuthenticationTask = new Task(()=> Console.WriteLine("task"));
		private static string authErrorMessage = "One or more errors occurred in Passive Authentication";
		private static Task dummyPassiveFailTask = new Task(()=> {throw new Exception(authErrorMessage);});

		private static KidoApplication currentApplication;
        private static WebSocket websocket = new WebSocket("wss://kidowebsocket-tasks-kidodemo.kidocloud.com/");
            
		public static Task Authenticate(this KidoApplication app, Context context) {
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

        
        public static void Subscribe(this Kidozen.KidoApplication app) {
             
            websocket.Opened += (obj, args) => {
                var message = "bindToChannel::{\"application\":\"local\",\"channel\":\"x-channel\"}";
                
                websocket.Send(message);
                Console.WriteLine("opened");
            };
            websocket.AllowUnstrustedCertificate = true;
            websocket.DataReceived += (obj, args) =>
            {
                Console.WriteLine("Data");
            };
            websocket.Error += (obj, args) => { 
                Console.WriteLine("Error");
            };
            websocket.Closed += (obj, args) => { 
                Console.WriteLine("Closed");
            };
            websocket.MessageReceived += (obj, args) =>
            {
                Console.WriteLine("MessageReceived");
            };
            
            websocket.Open();

            
        }
	}
}

