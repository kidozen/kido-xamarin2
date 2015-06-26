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

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

namespace Kidozen.iOS
{   
	public static partial class KidozenExtensions {
        static BreadCrumbs breadcrumbs = new BreadCrumbs();

        public static void AddCrashBreadCrumb(this Kidozen.KidoApplication app, string value) {
            breadcrumbs.Add(value);
        }

		public static void EnableCrash(this Kidozen.KidoApplication app) {
			AppDomain.CurrentDomain.UnhandledException+= delegate(object sender, UnhandledExceptionEventArgs e) {
				var ex = e.ExceptionObject as Exception;
				var stack = new System.Diagnostics.StackTrace(ex,true);
				var frame = stack.GetFrame(0);
				var filename = frame.GetFileName();
				var linenumber = frame.GetFileLineNumber();
				var methodname = frame.GetMethod().Name;
				var classname = frame.GetMethod().DeclaringType.FullName;
                var fullstack = ex.StackTrace;
				var reason = ex.GetType().Name;
				var appVersionCode = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();

				var message = C.Crash.CreateCrashMessage("monotouch",
					UIDevice.CurrentDevice.Name ,
					UIDevice.CurrentDevice.SystemName ,
					UIDevice.CurrentDevice.SystemVersion, 
					filename ,
					linenumber , 
					methodname ,
					classname ,
					fullstack ,
					reason,
					appVersionCode,
					appVersionCode,
                    breadcrumbs.GetAll().ToArray()
                    );

				storeCrash(message);
			};
			processPending (app.marketplace, app.application, app.key);
		}

		private static void processPending(string marketplace, string application, string key) {
			getCrashPending ().ToList().ForEach(m => send(m,marketplace,application,key));
		}

		private static void storeCrash (string crash) {
			var filename = String.Format ("{0}.crash", System.Guid.NewGuid ().ToString ());
			var documents = FileUtilities.GetDocumentsFolder ();
			System.IO.File.WriteAllText (Path.Combine (documents, filename), crash);
		}

		private static IEnumerable<string> getCrashPending() {
			return Directory.EnumerateFiles (FileUtilities.GetDocumentsFolder(), "*.crash");
		}

		private static void send (string crashpath, string marketplace, string application, string key) {
			var crash = System.IO.File.ReadAllText (crashpath);
			C.Crash.Create (crash, marketplace, application, key).ContinueWith(
				t=> {
					if (t.Result) System.IO.File.Delete (crashpath);						
				}
			);


		}
	}
}

