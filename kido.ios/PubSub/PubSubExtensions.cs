using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;

using SocketIOClient;

using A = KzApplication;

namespace Kidozen.iOS
{
    public class MyTransport : SocketIOClient.IClientTransport
    {

        public string Name
        {
            get { return "MyTransport"; }
        }

        public bool SupportsKeepAlive
        {
            get { return true; }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    public static partial class KidozenExtensions {
        private static ClientWebSocket webSocket = new ClientWebSocket();
        private static SocketIOClient.Client soClient = null;

        public static Task Subscribe(this Kidozen.KidoApplication app, string channel) 
        {
            var url = A.fetchConfigValue("ws", app.marketplace, app.application, app.key);
            webSocket = new ClientWebSocket();
            return webSocket.ConnectAsync(new Uri(url.Result), CancellationToken.None)
                .ContinueWith(t=> {
                    byte[] buffer = new byte[receiveChunkSize];
                    webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)
                        .ContinueWith(
                            tr =>
                            {
                                Console.WriteLine(tr.Result);
                            }
                );
           
            });
        }

        public static void Subscribe2(this Kidozen.KidoApplication app, string channel)
        {
            var url = A.fetchConfigValue("ws", app.marketplace, app.application, app.key).Result; //.Trim("/local".ToCharArray());
            soClient = new SocketIOClient.Client(url);
           

            soClient.Message += (obj, e) => {  
                Console.WriteLine(e.Message); 
            };

            soClient.Opened += (obj, e) =>
            {
                Console.WriteLine(e.ToString());
            };

            soClient.Error += (obj, e) =>
            {
                Console.WriteLine(e.Message);
            };

            soClient.SocketConnectionClosed += (obj, e) =>
            {
                Console.WriteLine(e.ToString());
            };

            soClient.Connect();
            return ;
        }



        static UTF8Encoding encoder = new UTF8Encoding();
        private static object consoleLock = new object();
        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 256;
        private const bool verbose = true;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(30000);
        private static async Task Send(ClientWebSocket webSocket)
        {

        }

    }
}
