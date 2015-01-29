using System;
using System.Globalization;
using System.Threading.Tasks;
using Kidozen.iOS.Analytics;
using Newtonsoft.Json;

#if __UNIFIED__
using Foundation;
using UIKit;

#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public static partial class KidozenAnalyticsExtensions
	{
	    private static AnalyticsSession _analyticsSession;
	    private static IDeviceStorage _deviceStorage;
	    private static IDeviceInformation _deviceInformation;


#if __ANDROID__
        public static void EnableAnalytics(this Kidozen.KidoApplication app) {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillEnterForegroundNotification, WillEnterForegroud);
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidEnterBackgroundNotification, DidEnterBackground);

	        _analyticsSession = AnalyticsSession.GetInstance(app.GetIdentity);
	        _analyticsSession.New();
		}
#else
        public static void EnableAnalytics(this Kidozen.KidoApplication app)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillEnterForegroundNotification, WillEnterForegroud);
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidEnterBackgroundNotification, DidEnterBackground);
            _deviceStorage = new DeviceStorage();
            _deviceInformation = new DeviceInformation();
            _analyticsSession = AnalyticsSession.GetInstance(app.GetIdentity);
            _analyticsSession.New(_deviceInformation);
        }

        private static void WillEnterForegroud(NSNotification obj)
        {
            var value = FileUtilities.GetNsUserDefaultStoredValue("SessionSavedDateTime");
            var savedDateTime = DateTime.Parse(value);

            _analyticsSession.RestoreFromDisk(_deviceStorage,savedDateTime);
        }

        private static void DidEnterBackground(NSNotification obj)
        {
            var savedDateTime = DateTime.Now;
            FileUtilities.SetNsUserDefaultStoredValue("SessionSavedDateTime", savedDateTime.ToString(CultureInfo.InvariantCulture));
            _analyticsSession.SaveToDisk(_deviceStorage);
        }
#endif
        
        public static Task Stop(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Stop();
                return;
            });
        }

        public static Task Pause(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Pause();
                return;
            });
        }

        public static Task Resume(this Kidozen.KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Resume();
                return;
            });
        }

        public static Task TagClick(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession
                    .AddValueEvent(new ValueEvent { eventName = "Click", eventValue = message });
                return;
            });
        }
        
        public static Task TagView(this Kidozen.KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession
                    .AddValueEvent(new ValueEvent { eventName = "View", eventValue = message });
                
                return;
            });
        }

        public static Task TagCustom<T>(this Kidozen.KidoApplication app,  T message)
        {
            return Task.Factory.StartNew(() =>
            {
                var serialized = JsonConvert.SerializeObject(message);
                _analyticsSession
                    .AddValueEvent(new ValueEvent { eventName = "CustomEvent", eventValue = serialized });
                
                return;
            });
        }

	}
}

