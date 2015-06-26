using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;
using AOS=Android.OS ;

namespace Kidozen.Android
{
	public static partial class KidozenExtensions
	{
        static BreadCrumbs breadcrumbs = new BreadCrumbs();

        public static void AddCrashBreadCrumb(this Kidozen.KidoApplication app, string value)
        {
            breadcrumbs.Add(value);
        }
		public static void EnableCrash(this KidoApplication app,Context context) {
			AndroidEnvironment.UnhandledExceptionRaiser += delegate(object sender, RaiseThrowableEventArgs e) {
				var ex = e.Exception;
				var stack = new StackTrace(ex,true);
				var frame = stack.GetFrame(0);
				var filename = frame.GetFileName();
				var linenumber = frame.GetFileLineNumber();
				var methodname = frame.GetMethod().Name;
				var classname = frame.GetMethod().DeclaringType.FullName;
				var fullstack = ex.StackTrace.Replace("\n",String.Empty);
				var reason = ex.GetType().Name;
				var appVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MetaData).VersionCode;
				var appVersionName = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MetaData).VersionName;

				var message = Crash.Crash.CreateCrashMessage("monotouch",
					AOS.Build.Device,
					AOS.Build.Model,
					AOS.Build.VERSION.Release, 
					filename ,
					linenumber , 
					methodname ,
					classname ,
					fullstack ,
					reason,
					appVersionName,
                    appVersionCode.ToString(),
                    breadcrumbs.GetAll().ToArray());

				storeCrash(message);
				Debug.WriteLine("about to exit application");
				AOS.Process.KillProcess(AOS.Process.MyPid());

			};
			processPending (app.marketplace, app.application, app.key);
		}


		private static void processPending(string marketplace, string application, string key) {
			getCrashPending ().ToList().ForEach(m => send(m,marketplace,application,key));
		}

		private static void storeCrash (string crash) {
			var filename = String.Format ("{0}.crash", Guid.NewGuid ().ToString ());
			var documents = AOS.Environment.ExternalStorageDirectory.ToString ();
			System.IO.File.WriteAllText (Path.Combine (documents, filename), crash);
		}

		private static IEnumerable<string> getCrashPending() {
			var documents = AOS.Environment.ExternalStorageDirectory.ToString ();
			return Directory.EnumerateFiles (documents, "*.crash");
		}

		private static void send (string crashpath, string marketplace, string application, string key) {
			var crash = System.IO.File.ReadAllText (crashpath);
			Crash.Crash.Create (crash, marketplace, application, key).ContinueWith(
					t=> {
						if (t.Result) System.IO.File.Delete (crashpath);						
					}
			);

		}
	}
}
