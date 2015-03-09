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
        private WebSocket websocket;
        public event PubSubMessageArrivedDelegate OnMessageEvent;
        static NSObject caller = new NSObject();
        private string _name = string.Empty;   
        public PubSubChannel()
        {
            Console.WriteLine("Constructor  ...");
        }

        public bool Subscribe(string endpoint, string name)
        {
            var connectionSuccess = true;
            _name = name;
            PubSubChannel<T>.caller.InvokeOnMainThread(() =>
            {
                websocket = new WebSocket(endpoint);
                websocket.AllowUnstrustedCertificate = true;
                websocket.EnableAutoSendPing = true;
                websocket.AutoSendPingInterval = 1;
                websocket.Opened += OnConnect;
                websocket.MessageReceived += OnMessageReceived;

                websocket.Error += (obj, args) =>
                {
                    Console.WriteLine("Error, " + args.Exception.Message.ToString());
                    connectionSuccess = false;
                };

                websocket.Closed += (obj, args) =>
                {
                    Console.WriteLine("Closed");
                };

                
                websocket.Open();
            });
            return connectionSuccess;
        }

        internal void OnConnect(object sender, EventArgs args)
        {
            Console.WriteLine("Opened");
            var message = "bindToChannel::{\"application\":\"local\",\"channel\":\"" + _name + "\"}";
            websocket.Send(message);
            Console.WriteLine("Bind: " + message);
        }

        internal void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine("MessageReceived: " + args.Message);
            
            if (this.OnMessageEvent != null)
            {
                var eom = args.Message.IndexOf("::") + 2;
                var jsonmessage = args.Message.Substring(eom, args.Message.Length - eom);
                if (typeof(T)!= typeof( System.String ) )
                {
                    var message = JsonConvert.DeserializeObject<T>(jsonmessage);
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<T>(message, true ));
                }
                else
                {
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<string>(jsonmessage,true ));
                }

            }
            
        }
    }
}