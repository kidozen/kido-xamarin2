using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if __ANDROID__

namespace Kidozen.Android

#else
using Foundation;
using UIKit;

namespace Kidozen.iOS

#endif
{

    public class PubSubChannelEventArgs<A> : System.EventArgs
    {
        public Boolean Success { get; set; }
        public A Value { get; set; }
    }

    public class PubSubChannel<T> : Kidozen.ISubscriber
    {
        private WebSocket websocket;
        public event PubSubMessageArrivedDelegate OnMessageEvent;
        private string _name = string.Empty;   
        public PubSubChannel(){}

        public bool Subscribe(string endpoint, string name)
        {
            var success = true;
            try
            {
                _name = name;
                websocket = new WebSocket(endpoint);
                websocket.AllowUnstrustedCertificate = true;
                websocket.EnableAutoSendPing = true;
                websocket.AutoSendPingInterval = 1;
                websocket.Opened += OnConnect;
                websocket.MessageReceived += OnMessageReceived;

                websocket.Error += OnError;
                // TODO: Check why there is an error with this lib
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true; System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                websocket.Open();
            }
            catch (Exception)
            {
                success = false;
            }
            
            return success;
        }


        private void OnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine(e.Exception.Message);
            }
            if (this.OnMessageEvent != null) {
                if (e.Exception.Message.ToLower().IndexOf("you must send data by websocket after websocket is opened") == -1)
                {
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<SuperSocket.ClientEngine.ErrorEventArgs> { Value = e, Success = false });
                }
            }
        }

        internal void OnConnect(object sender, EventArgs args)
        {
            var message = "bindToChannel::{\"application\":\"local\",\"channel\":\"" + _name + "\"}";
            websocket.Send(message);
            Console.WriteLine("Bind: " + message);
        }

        internal void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (this.OnMessageEvent != null)
            {
                var eom = args.Message.IndexOf("::") + 2;
                var jsonmessage = args.Message.Substring(eom, args.Message.Length - eom);
                if (typeof(T)!= typeof( System.String ) )
                {
                    var message = JsonConvert.DeserializeObject<T>(jsonmessage);
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<T> { Value = message, Success = true } );
                }
                else
                {
                    this.OnMessageEvent.Invoke(this, new PubSubChannelEventArgs<string>{ Value = jsonmessage, Success = true });
                }

            }
            
        }
    }
}