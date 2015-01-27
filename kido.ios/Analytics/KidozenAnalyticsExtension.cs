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
	public static partial class KidozenAnalyticsExtensions
	{
        
		public static void EnableAnalytics(this Kidozen.KidoApplication app) {
            AnalyticsSession.GetInstance().New();
		}

        public static Task TagClick(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                return ;
            });		
        }

        public static Task TagView(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                return;
            });
        }

        public static Task TagCustom<T>(this Kidozen.KidoApplication app, T message)
        {
            return Task.Factory.StartNew(() =>
            {
                return;
            });
        }

		private static void processPendingAnalytics(string marketplace, string application, string key) {
            getAnalyticsPendingData().ToList().ForEach(m => sendAnalyticsData(m, marketplace, application, key));
		}

		private static void storeAnalyticsData (string crash) {
			var filename = String.Format ("{0}.crash", System.Guid.NewGuid ().ToString ());
			var documents = FileUtilities.getDocumentsFolder ();
			System.IO.File.WriteAllText (Path.Combine (documents, filename), crash);
		}

		private static IEnumerable<string> getAnalyticsPendingData() {
			return Directory.EnumerateFiles (FileUtilities.getDocumentsFolder (), "*.crash");
		}

		private static void sendAnalyticsData (string crashpath, string marketplace, string application, string key) {
			var crash = System.IO.File.ReadAllText (crashpath);
			C.Crash.Create (crash, marketplace, application, key).ContinueWith(
				t=> {
					if (t.Result) System.IO.File.Delete (crashpath);						
				}
			);
		}
	}
}

