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
        private ManualResetEvent subscribeEvent = new ManualResetEvent(false);
        private int subscriptionTimeout = 10000;
        private WebSocket websocket ;
        public event PubSubMessageArrivedDelegate OnMessageEvent;
        
        public bool Subscribe(string endpoint, string name)
        {
            this.websocket = new WebSocket(endpoint);
            websocket.AllowUnstrustedCertificate = true;
            var connectionSuccess = false;
            websocket.Opened += (obj, args) =>
            {
                var message = "bindToChannel::{\"application\":\"local\",\"channel\":\"" + name +"\"}";
                websocket.Send(message);
                connectionSuccess = true;
                subscribeEvent.Set();
            };

            websocket.Error += (obj, args) =>
            {
                Console.WriteLine("Error");
            };

            websocket.Closed += (obj, args) =>
            {
                Console.WriteLine("Closed");
            };
            
            websocket.MessageReceived += (obj, args) =>
            {
                Console.WriteLine("MessageReceived: " + args.ToString() );
            };

            websocket.Open();

            WaitHandle.WaitAll(new WaitHandle[] { subscribeEvent }, subscriptionTimeout);
            return connectionSuccess;
        }
    }
}