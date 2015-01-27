using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Timers;

namespace Kidozen.iOS
{
    public class AnalyticsSession
    {
        const double DEFAULT_TIMER_INTERVAL = 5 * 60 * 1000;
        Timer timerUploader = new Timer(DEFAULT_TIMER_INTERVAL);
        String currentSessionId = System.Guid.NewGuid().ToString();
        List<Event> sessionEvents = new List<Event>();
        SessionEvent mainEvent;

        public AnalyticsSession()
        {
            timerUploader.Elapsed += timerUploader_Elapsed;
        }

        void timerUploader_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void New() {
            var eventAttributes = new SessionAttributes { isoCountryCode = "AR" };
            mainEvent = new SessionEvent { sessionUUID = currentSessionId, eventAttr = eventAttributes, StartDate = DateTime.Now.Ticks };
        }

        public void AddValueEvent(ValueEvent evt)
        {
            sessionEvents.Add(evt);
        }

        public void Stop() {
            mainEvent.EndDate = DateTime.Now.Ticks;

            sessionEvents.Add(mainEvent);
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