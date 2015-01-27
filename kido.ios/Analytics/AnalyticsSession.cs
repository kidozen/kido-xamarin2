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
        SessionAttributes eventAttributes;

        long startDate = 0 ;

        
        static volatile AnalyticsSession instance;
        static object syncRoot = new Object();

        Analytics kidoAnalyticsEp;
        KzApplication.Identity identity;

        public static AnalyticsSession GetInstance(KzApplication.Identity identity)
        {
            lock (syncRoot)
            {
                if (instance == null)
                {
                    instance = new AnalyticsSession(identity);                    
                }
            }
            return instance;
        }

        public AnalyticsSession(KzApplication.Identity identity)
        {
            this.identity = identity;
            this.kidoAnalyticsEp = new Analytics(this.identity);
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
            kidoAnalyticsEp.SaveSession(sessionEvents)
                .ContinueWith(
                    t => {
                        if (!t.IsFaulted)
                        {
                            Console.WriteLine("Cleanup");
                        }
                    }
                );

        }

        public void New() {
            timerUploader.Enabled = true;
            timerUploader.Start();

            eventAttributes = new SessionAttributes
            { 
                isoCountryCode = "AR",
                platform = "iOS",
                networkAccess = "Wifi",
                carrierName = "Personal",
                systemVersion = "1.0",
                deviceModel = "X"
            };
            startDate = DateTime.UtcNow.Ticks;
        }

        public void AddValueEvent(ValueEvent evt)
        {
            evt.sessionUUID = this.currentSessionId;
            sessionEvents.Add(evt);
        }

        public void Stop() {
            timerUploader.Enabled = false;
            timerUploader.Stop();

            var end = DateTime.UtcNow;
            var lenght = end.Subtract( new DateTime(startDate));

            var mainEvent = new SessionEvent
            {
                sessionUUID = currentSessionId,
                eventAttr = eventAttributes,
                StartDate = startDate,
                EndDate = end.Ticks,
                length = lenght.Ticks
            }; 

            sessionEvents.Add(mainEvent);
            this.doUpload();
        }

        public void Resume(){
            timerUploader.Enabled = true;
            timerUploader.Start();
        }

        public void Pause(){
            timerUploader.Enabled = false;
            timerUploader.Stop();
        }

        private void Reset()
        {
            this.currentSessionId = System.Guid.NewGuid().ToString();
            sessionEvents = new List<Event>();
            if (!timerUploader.Enabled)
            {
                timerUploader.Enabled = false;  
            } 
        }
     }
}