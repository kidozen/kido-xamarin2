using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Foundation;
using UIKit;
using System.Diagnostics.Tracing;

namespace Kidozen.iOS
{
    public class EventManager : WeakEventManager
    {
        private static EventManager CurrentManager
        {
            get
            {
                EventManager manager = (EventManager)GetCurrentManager(typeof(EventManager));

                if (manager == null)
                {
                    manager = new EventManager();
                    SetCurrentManager(typeof(EventManager), manager);
                }

                return manager;
            }
        }


        public static void AddListener(EventSource source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(EventSource source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        protected override void StartListening(object source)
        {
            ((EventSource)source).Event += DeliverEvent;
        }

        protected override void StopListening(object source)
        {
            ((EventSource)source).Event -= DeliverEvent;
        }
    }

    public class LegacyWeakEventListener : IWeakEventListener
    {
        private void OnEvent(object source, EventArgs args)
        {
            Console.WriteLine("LegacyWeakEventListener received event.");
        }

        public LegacyWeakEventListener(EventSource source)
        {
            EventManager.AddListener(source, this);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            OnEvent(sender, e);

            return true;
        }

        ~LegacyWeakEventListener()
        {
            Console.WriteLine("LegacyWeakEventListener finalized.");
        }
    }
}