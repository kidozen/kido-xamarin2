using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.FSharp.Core;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using K = Kidozen;
using U = Utilities;
using A = Application;
using C = Crash;

namespace Kidozen.iOS
{
	public static partial class KidozenExtensions
	{
		public static void EnableCrash(this Kidozen.KidoApplication app) {
			AppDomain.CurrentDomain.UnhandledException+= delegate(object sender, UnhandledExceptionEventArgs e) {
				var ex = e.ExceptionObject as Exception;
				var stack = new System.Diagnostics.StackTrace(ex,true);
				var frame = stack.GetFrame(0);
				var filename = frame.GetFileName();
				var linenumber = frame.GetFileLineNumber();
				var methodname = frame.GetMethod().Name;
				var classname = frame.GetMethod().DeclaringType.FullName;
				var fullstack = ex.StackTrace.Replace("\n",String.Empty);
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
					appVersionCode);

				storeCrash(message);
			};
			processPending (app.marketplace, app.application, app.key);
		}


		private static void processPending(string marketplace, string application, string key) {
			getCrashPending ().ToList().ForEach(m => send(m,marketplace,application,key));
		}

		private static string getDocumentsFolder() {
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
				var folders = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User);
				return folders[0].Path;
			} else {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				return Path.GetFullPath(Path.Combine (documents, "..", "Library","Caches"));
			}
		}

		private static void storeCrash (string crash) {
			var filename = String.Format ("{0}.crash", System.Guid.NewGuid ().ToString ());
			var documents = getDocumentsFolder ();
			File.WriteAllText (Path.Combine (documents, filename), crash);
		}

		private static IEnumerable<string> getCrashPending() {
			return Directory.EnumerateFiles (getDocumentsFolder (), "*.crash");
		}

		private static void send (string crashpath, string marketplace, string application, string key) {
			var crash = File.ReadAllText (crashpath);
			C.Crash.Create (crash, marketplace, application, key).ContinueWith(
				t=> {
					if (t.Result) File.Delete (crashpath);						
				}
			);


		}
	}
}

