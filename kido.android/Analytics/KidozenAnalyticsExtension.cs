using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Android.App;
using Kidozen.Analytics;
using Kidozen.Analytics.Android;

namespace Kidozen.Android
{
    public static partial class KidozenAnalyticsExtensions
    {
        private static AnalyticsSession _analyticsSession;
        private static AnalyticsActivityLifecycleCallbacks LifecycleCallbacks = 
            new AnalyticsActivityLifecycleCallbacks();

        private static void DidEnterBackground(Double seconds)
        {
            Console.WriteLine(seconds.ToString());
        }

        public static void EnableAnalytics(this Kidozen.KidoApplication app, Application application)
        {

            var deviceStorage = new DeviceStorage();
            var deviceInformation = new DeviceInformation(application.ApplicationContext);
            _analyticsSession = AnalyticsSession.GetInstance(app.GetIdentity);

            _analyticsSession.New(deviceInformation);

            var lifecycleCallbacks = new AnalyticsActivityLifecycleCallbacks();
            LifecycleCallbacks.BackgroundCallback(DidEnterBackground);
            application.RegisterActivityLifecycleCallbacks(lifecycleCallbacks);

        }

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

        public static Task TagCustom<T>(this KidoApplication app, T message)
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

