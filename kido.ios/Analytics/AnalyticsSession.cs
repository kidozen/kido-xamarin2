using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Timers;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kidozen.iOS
{
    public class AnalyticsSession
    {
        const double DEFAULT_TIMER_INTERVAL = 5 * 1000; //* 60 
        Timer timerUploader = new Timer(DEFAULT_TIMER_INTERVAL);
        String currentSessionId = System.Guid.NewGuid().ToString();
        List<Event> sessionEvents = new List<Event>();
        SessionEvent mainEvent;

        private static volatile AnalyticsSession instance;
        private static object syncRoot = new Object();
       
        public static AnalyticsSession GetInstance()
        {
            lock (syncRoot)
            {
                if (instance == null)
                {
                    instance = new AnalyticsSession();
                }
            }
            return instance;
        }

        public AnalyticsSession()
        {
            timerUploader.Elapsed += timerUploader_Elapsed;

        }

        void timerUploader_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sessionEvents.Count > 0)
            {
                doUpload();
            }

        }

        private void doUpload()
        {
            var jsonmessage = JsonConvert.SerializeObject(sessionEvents);
            Console.WriteLine(jsonmessage);
        }

        public void New() {
            timerUploader.Enabled = true;
            timerUploader.Start();

            var eventAttributes = new SessionAttributes { 
                isoCountryCode = "AR",
                platform = "iOS",
                networkAccess = "Wifi",
                carrierName = "Personal",
                systemVersion = "1.0",
                deviceModel = "X"
            };
            mainEvent = new SessionEvent { 
                sessionUUID = currentSessionId, 
                eventAttr = eventAttributes, 
                StartDate = DateTime.UtcNow.Ticks };
        }

        public void AddValueEvent(ValueEvent evt)
        {
            sessionEvents.Add(evt);
        }

        public void Stop() {
            timerUploader.Enabled = false;
            timerUploader.Stop();

            var end = DateTime.UtcNow;
            var lenght = end.Subtract( new DateTime(mainEvent.EndDate));

            mainEvent.EndDate = end.Ticks;
            mainEvent.length = lenght.Ticks;
            sessionEvents.Add(mainEvent);

            this.doUpload();

        }

        public void Resume(){

        }

        public void Pause(){

        }

        private void Reset()
        {
            this.currentSessionId = System.Guid.NewGuid().ToString();
            sessionEvents = new List<Event>();
            timerUploader.Enabled = false;
        }
     }
}