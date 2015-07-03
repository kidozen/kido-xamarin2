using K = Kidozen;
using U = Utilities;
using A = KzApplication;

#if __ANDROID__

namespace Kidozen.Android

#else

namespace Kidozen.iOS

#endif
{

    public static partial class KidozenExtensions
    {
        private static PubSub psInstance;
        
        public static PubSub SetupChannel<T>(this KidoApplication app, string name, 
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