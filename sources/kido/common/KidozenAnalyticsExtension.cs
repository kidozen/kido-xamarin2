﻿using System;
using System.Globalization;
using System.Threading.Tasks;

using Newtonsoft.Json;

#if __ANDROID__
using Android.Content;
using Kidozen.Android.Analytics;
#else
using Kidozen.iOS.Analytics;

#if __UNIFIED__
    using Foundation;
    using UIKit;
using Kidozen.Analytics;
#else
#endif
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
        public static void EnableAnalytics(this Kidozen.KidoApplication app, Context context) {
            _deviceStorage = new DeviceStorage();
            _deviceInformation = new DeviceInformation(context);
            _analyticsSession = AnalyticsSession.GetInstance(app.GetIdentity);
            _analyticsSession.New(_deviceInformation);
        }
#else
        public static void EnableAnalytics(this KidoApplication app)
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
        
        public static Task Stop(this KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Stop();
                return;
            });
        }

        public static Task Pause(this KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Pause();
                return;
            });
        }

        public static Task Resume(this KidoApplication app)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession.Resume();
                return;
            });
        }

        public static Task TagClick(this KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession
                    .AddValueEvent(new ValueEvent { eventName = "Click", eventValue = message });
                return;
            });
        }
        
        public static Task TagView(this KidoApplication app, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                _analyticsSession
                    .AddValueEvent(new ValueEvent { eventName = "View", eventValue = message });
                
                return;
            });
        }

        public static Task TagCustom<T>(this KidoApplication app, string title,  T message)
        {
            return Task.Factory.StartNew(() =>
            {
                var serialized = JsonConvert.SerializeObject(message);
                _analyticsSession
                    .AddCustomEvent(new CustomEvent<T> { eventName = title, eventAttr = message });
                
                return;
            });
        }

	}
}

