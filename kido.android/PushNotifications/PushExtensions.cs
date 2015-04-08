using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kidozen;
using System.Threading.Tasks;

using Kidozen;
using Newtonsoft.Json;

namespace Kidozen.Android
{
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
            return n.Push<PushNotification>(new InternalPushNotification(message));
        }

        public static Task<Boolean> Unsubscribe(this Kidozen.KidoApplication app, string channel, string subscriptionId)
        {
            var n = new Notifications(app.application, channel, subscriptionId, app.GetIdentity);
            return n.UnSubscribe();
        }


        public static Task<List<SubscriptionDetails>> GetApplicationSubscriptions(this Kidozen.KidoApplication app, string DeviceId)
        {
            var n = new Notifications(app.application, string.Empty, DeviceId, app.GetIdentity);
            return n.GetSubscriptions().ContinueWith(t =>
            {
                var list = t.Result;
                var subscriptionsAsList = new List<SubscriptionDetails>();
                if (!string.Equals(list, "[]"))
                {
                    subscriptionsAsList = JsonConvert
                        .DeserializeObject<IEnumerable<SubscriptionDetails>>(list)
                        .ToList<SubscriptionDetails>();
                }

                return subscriptionsAsList;
            });
        }

        public static Task<List<string>> GetApplicationChannels(this Kidozen.KidoApplication app, string DeviceId)
        {
            var n = new Notifications(app.application, string.Empty, DeviceId, app.GetIdentity);
            return n.GetApplicationChannels().ContinueWith(t =>
            {
                var list = t.Result;
                var channels = new List<string>();
                if (!string.Equals(list, "[]"))
                {
                    channels = JsonConvert
                        .DeserializeObject<IEnumerable<string>>(list)
                        .ToList<string>();
                }

                return channels;
            });
        }


    }
}