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
    public class PushNotification {
        public String type {get;set;}
        public String title { get; set; }
        public String text { get; set; }
        public String image { get; set; }
        public int badge { get; set; } 
    }

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
            KidozenExtensions.deviceToken = deviceToken;
            var n = new Notifications(app.application, name, KidozenExtensions.deviceToken, app.GetIdentity);
            return n.Subscribe(
                    new 
                    { 
                        platform = "apns",
                        subscriptionId = deviceToken,
                        deviceId = UniqueIdentification
                    }
                );
        }

        public static Task<Boolean> PushToChannel(this Kidozen.KidoApplication app, string name, PushNotification message)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.deviceToken))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.deviceToken, app.GetIdentity);
            return n.Push<PushNotification>(message);
        }

        public static Task<Boolean> Unsubscribe(this Kidozen.KidoApplication app, string name)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.deviceToken))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.deviceToken, app.GetIdentity);
            return n.UnSubscribe();
        }

        public static Task<IEnumerable<object>> GetSubscriptions(this Kidozen.KidoApplication app, string name)
        {
            if (String.IsNullOrEmpty(KidozenExtensions.deviceToken))
            {
                throw new Exception("You must first call 'SubscribeToChannel'");
            }
            var n = new Notifications(app.application, name, KidozenExtensions.deviceToken, app.GetIdentity);
            return n.GetSubscriptions();
        }
    }
}