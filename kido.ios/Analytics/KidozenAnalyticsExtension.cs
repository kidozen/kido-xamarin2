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

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kidozen.iOS
{
	public static partial class KidozenAnalyticsExtensions
	{
        public static void EnableAnalytics(this Kidozen.KidoApplication app) {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillEnterForegroundNotification, willEnterForegroud);
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidEnterBackgroundNotification, didEnterBackground);

            AnalyticsSession.GetInstance(app.GetIdentity).New();
		}

        private static void willEnterForegroud(NSNotification obj)
        {
            throw new NotImplementedException();
        }
        private static void didEnterBackground(NSNotification obj)
        {
            throw new NotImplementedException();
        }

        public static Task Stop(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                AnalyticsSession.GetInstance(app.GetIdentity)
                    .Stop();
                return;
            });
        }

        public static Task Pause(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                AnalyticsSession.GetInstance(app.GetIdentity)
                    .Pause();
                return;
            });
        }

        public static Task Resume(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                AnalyticsSession.GetInstance(app.GetIdentity)
                    .Resume();
                return;
            });
        }

        public static Task TagClick(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                AnalyticsSession.GetInstance(app.GetIdentity)
                    .AddValueEvent(new ValueEvent { eventName = "Click", eventValue = message });
                return;
            });
        }
        
        public static Task TagView(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                AnalyticsSession.GetInstance(app.GetIdentity).AddValueEvent(new ValueEvent { eventName = "View", eventValue = message });
                
                return;
            });
        }

        public static Task TagCustom<T>(this Kidozen.KidoApplication app,  T message)
        {
            return Task.Factory.StartNew(() =>
            {
                var serialized = JsonConvert.SerializeObject(message);
                AnalyticsSession.GetInstance(app.GetIdentity)
                    .AddValueEvent(new ValueEvent { eventName = "CustomEvent", eventValue = serialized });
                
                return;
            });
        }

		private static void processPendingAnalytics(string marketplace, string application, string key) {
            getAnalyticsPendingData().ToList().ForEach(m => sendAnalyticsData(m, marketplace, application, key));
		}

		private static void storeAnalyticsData (string crash) {
			var filename = String.Format ("{0}.crash", System.Guid.NewGuid ().ToString ());
			var documents = FileUtilities.GetDocumentsFolder ();
			System.IO.File.WriteAllText (Path.Combine (documents, filename), crash);
		}

		private static IEnumerable<string> getAnalyticsPendingData() {
			return Directory.EnumerateFiles (FileUtilities.GetDocumentsFolder (), "*.crash");
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

