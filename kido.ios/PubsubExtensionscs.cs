using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using K = Kidozen;
using U = Utilities;
using A = KzApplication;

namespace Kidozen.iOS
{

    public static partial class KidozenExtensions
    {
        private static PubSub psInstance;
        
        public static PubSub SubscribeToChannel<T>(this Kidozen.KidoApplication app, string name, 
            PubSubMessageArrivedDelegate onMessageArrive)
        {
            psInstance = app.PubSub(name);
            psInstance.SubscriberInstance = new PubSubChannel<T>();
            psInstance.SubscriberInstance.OnMessageEvent += onMessageArrive;
            //
            psInstance.Subscribe();
            return psInstance;
        }

        
    }
}