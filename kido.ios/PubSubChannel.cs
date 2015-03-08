using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Foundation;
using UIKit;

using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kidozen.iOS
{
    public class PubSubChannel<T> : Kidozen.ISubscriber
    {
        private WebSocket websocket ;
        public event PubSubMessageArrivedDelegate OnMessageEvent;

        public PubSubChannel()
        {
            Console.WriteLine("Constructor  ..." );
        }

        public bool Subscribe(string endpoint, string name)
        {
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
            };

            websocket.Closed += (obj, args) =>
            {
                Console.WriteLine("Closed");
            };
            
            websocket.MessageReceived += (obj, args) =>
            {
                Console.WriteLine("MessageReceived: " + args.ToString() );
                if (this.OnMessageEvent!=null)
                {
                    var eom = args.Message.IndexOf("::") + 2;
                    var jsonmessage = args.Message.Substring(eom, args.Message.Length);
                    var message = JsonConvert.DeserializeObject<T>(jsonmessage);
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<T>(message,true));
                }
            };

            websocket.Open();
            });
            return connectionSuccess;
        }
    }
}