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
        public static Task<Boolean> SubscribeToChannel(this Kidozen.KidoApplication app, string channel, string subscriptionId, string deviceId)
        {
            var n = new Notifications(app.application, channel, subscriptionId, app.GetIdentity);
            return n.Subscribe(
                    new
                    {
                        platform = "gcm",
                        subscriptionId = subscriptionId,
                        deviceId = deviceId
                    }
                );
        }

        public static Task<Boolean> PushToChannel(this Kidozen.KidoApplication app, string channel, string subscriptionId, PushNotification message)
        {
            var n = new Notifications(app.application, channel, subscriptionId, app.GetIdentity);
            return n.Push<PushNotification>(message);
        }

        public static Task<Boolean> Unsubscribe(this Kidozen.KidoApplication app, string channel, string subscriptionId)
        {
            var n = new Notifications(app.application, channel, subscriptionId, app.GetIdentity);
            return n.UnSubscribe();
        }

        public static Task<IEnumerable<SubscriptionDetails>> GetSubscriptions(this Kidozen.KidoApplication app, string deviceId)
        {
            var n = new Notifications(app.application, string.Empty, deviceId, app.GetIdentity);
            return n.GetSubscriptions();
        }
    }
}