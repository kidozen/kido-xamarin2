using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using WebSocket4Net;
using System.Threading;

namespace Kidozen.iOS
{
    public class PubSubChannel : Kidozen.ISubscriber
    {
        //private ManualResetEvent subscribeEvent = new ManualResetEvent(false);
        //private int subscriptionTimeout = 10000;
        private WebSocket websocket ;
        public event PubSubMessageArrivedDelegate OnMessageEvent;

        public PubSubChannel()
        {
            Console.WriteLine("Constructor  ..." );

        }

        public bool Subscribe(string endpoint, string name)
        {
            Console.WriteLine("Subscribe, " + endpoint + ", " + name );
            NSObject caller = new NSObject();
            var connectionSuccess = true;

            caller.InvokeOnMainThread( () => { 
            this.websocket = new WebSocket(endpoint);
            websocket.AllowUnstrustedCertificate = true;
            websocket.Opened += (obj, args) =>
            {
                Console.WriteLine("Opened");
                var message = "bindToChannel::{\"application\":\"local\",\"channel\":\"" + name +"\"}";
                Console.WriteLine("Bind: " + message);

                websocket.Send(message);

            };

            websocket.Error += (obj, args) =>
            {
                Console.WriteLine("Error, " + args.ToString());
                connectionSuccess = false;
          //      subscribeEvent.Set();
            };

            websocket.Closed += (obj, args) =>
            {
                Console.WriteLine("Closed");
            };
            
            websocket.MessageReceived += (obj, args) =>
            {
                Console.WriteLine("MessageReceived: " + args.ToString() );
            };

            Console.WriteLine("Opening ...");
            websocket.Open();
            Console.WriteLine("Opened");
            });
            //WaitHandle.WaitAll(new WaitHandle[] { subscribeEvent }, subscriptionTimeout);
            Console.WriteLine("Finished: " + connectionSuccess.ToString());

            return connectionSuccess;
        }
    }
}