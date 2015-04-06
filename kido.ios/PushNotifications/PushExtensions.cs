using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Kidozen;
using System.Threading.Tasks;

namespace Kidozen.iOS
{
    public static partial class KidozenExtensions
    {
        private static Notifications notificationsIntance;
        private static string deviceToken;

        private static string UniqueIdentification
        {
             get { 
                var value = NSUserDefaults.StandardUserDefaults.StringForKey("kUniqueIdentificationFilename"); 
                if (value == null) {
                    var id = Guid.NewGuid().ToString();
                    NSUserDefaults.StandardUserDefaults.SetString(id, "kUniqueIdentificationFilename"); 
                    NSUserDefaults.StandardUserDefaults.Synchronize ();
                    return id;
                }
                else 
                    return value;
            }
            set {
                NSUserDefaults.StandardUserDefaults.SetString(value.ToString (), "kUniqueIdentificationFilename"); 
                NSUserDefaults.StandardUserDefaults.Synchronize ();
            }
        }


        public static Task<Boolean> SubscribeToChannel(this Kidozen.KidoApplication app, string name, string deviceToken)
        {
            var n = new Notifications(app.application, name, deviceToken, app.GetIdentity);
            return n.Subscribe(
                    new 
                    { 
                        platform = "apns",
                        subscriptionId = deviceToken,
                        deviceId = UniqueIdentification
                    }
                );
        }

        public static Task<Boolean> PushToChannel(this Kidozen.KidoApplication app, string channel, string deviceToken, PushNotification message)
        {
            var n = new Notifications(app.application, channel, deviceToken, app.GetIdentity);
            return n.Push<PushNotification>(message);
        }

        public static Task<Boolean> Unsubscribe(this Kidozen.KidoApplication app, string channel, string deviceToken)
        {
            var n = new Notifications(app.application, channel, deviceToken, app.GetIdentity);
            return n.UnSubscribe();
        }

        public static Task<IEnumerable<SubscriptionDetails>> GetSubscriptions(this Kidozen.KidoApplication app, string deviceToken)
        {
            var n = new Notifications(app.application, string.Empty, deviceToken, app.GetIdentity);
            return n.GetSubscriptions();
        }
    }
}