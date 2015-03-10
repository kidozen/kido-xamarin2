using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kidozen;
using System.Threading.Tasks;

using Kidozen;

namespace Kidozen.Android
{
    public class PushNotification
    {
        public String type { get; set; }
        public String title { get; set; }
        public String text { get; set; }
        public String image { get; set; }
        public int badge { get; set; }
    }

    public static partial class KidozenExtensions
    {
        private static Notifications notificationsIntance;
        private static string subscriptionID;


        public static Task<Boolean> SubscribeToChannel(this Kidozen.KidoApplication app, string name, string subscriptionId, string deviceId)
        {
            KidozenExtensions.subscriptionID = subscriptionID;
            var n = new Notifications(app.application, name, KidozenExtensions.subscriptionID, app.GetIdentity);
            return n.Subscribe(
                    new
                    {
                        platform = "gcm",
                        subscriptionId = subscriptionId,
                        deviceId = deviceId
                    }
                );
        }

        public static Task<Boolean> PushToChannel(this Kidozen.KidoApplication app, string name, PushNotification message)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.subscriptionID))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.subscriptionID, app.GetIdentity);
            
            return n.Push<PushNotification>(message);
        }

        public static Task<Boolean> Unsubscribe(this Kidozen.KidoApplication app, string name)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.subscriptionID))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.subscriptionID, app.GetIdentity);
            return n.UnSubscribe();
        }

        public static Task<IEnumerable<object>> GetSubscriptions(this Kidozen.KidoApplication app, string name)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.subscriptionID))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.subscriptionID, app.GetIdentity);
            return n.GetSubscriptions();
        }
    }
}