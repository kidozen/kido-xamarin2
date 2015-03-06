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
        public static PubSub SubscribeToChannel(this Kidozen.KidoApplication app, string name)
        {
            var psInstance = app.PubSub(name);
            psInstance.SubscriberInstance = new PubSubChannel();

            //
            psInstance.Subscribe();
            return psInstance;
        }
    }
}