using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Kidozen;
using Newtonsoft.Json;

namespace Kido
{
    public class AnalyticsEp
    {
        public Analytics KidoAnalyticsEp;

        public AnalyticsEp(KzApplication.Identity identity)
        {
            this.KidoAnalyticsEp = new Analytics(identity);
        }
    }
}

namespace Kidozen.Analytics
{
    public class AnalyticsSession
    {
        const string Subfolder = "AnaliticsSessions";
        private const double DefaultTimerInterval = 1*1000*60; 
        readonly Timer _timerUploader = new Timer(DefaultTimerInterval);
        String _currentSessionId = Guid.NewGuid().ToString();
        List<Event> _sessionEvents = new List<Event>();
        SessionAttributes _eventAttributes;

        long _startDate = 0;

        static volatile AnalyticsSession _instance;
        static readonly object SyncRoot = new Object();
        private readonly Kido.AnalyticsEp _kidoAnalyticsEp;


        private const double DefaultSessionTimeoutInSeconds = 5;

        public static AnalyticsSession GetInstance(KzApplication.Identity identity)
        {
            lock (SyncRoot)
            {
                if (_instance == null)
                {
                    _instance = new AnalyticsSession(identity);
                }
            }
            return _instance;
        }

        public AnalyticsSession(KzApplication.Identity identity)
        {
            this._kidoAnalyticsEp = new Kido.AnalyticsEp(identity);
            _timerUploader.Elapsed += timerUploader_Elapsed;
        }

        void timerUploader_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Pause();
            if (_sessionEvents.Count <= 0)
            {
                this.Resume();
                return;
            }
            var message = JsonConvert.SerializeObject(_sessionEvents);
            DoUpload(message).ContinueWith
                (
                    t =>
                    {
                        if (!t.IsFaulted) this.Cleanup();
                        this.Resume();
                    }
                );
        }

        private void Cleanup()
        {
            _sessionEvents = new List<Event>();
        }

        private Task<bool> DoUpload(string message)
        {
            Console.WriteLine(message);
            return _sessionEvents.Count <= 0 ?  new Task<bool>(() => true) : _kidoAnalyticsEp.KidoAnalyticsEp.SaveSession(message);
        }

        public void New(IDeviceInformation information)
        {
            _timerUploader.Enabled = true;
            _timerUploader.Start();

            _eventAttributes = information.GetAttributes();
            _startDate = DateTime.UtcNow.Ticks;
        }

        public void AddValueEvent(ValueEvent evt)
        {
            evt.sessionUUID = this._currentSessionId;
            _sessionEvents.Add(evt);
        }

        public void Stop()
        {
            _timerUploader.Enabled = false;
            _timerUploader.Stop();

            var message = JsonConvert.SerializeObject(_sessionEvents);
            this.DoUpload(message);
        }

        public void Resume()
        {
            _timerUploader.Enabled = true;
            _timerUploader.Start();
        }

        public void Pause()
        {
            _timerUploader.Enabled = false;
            _timerUploader.Stop();
        }

        public void SaveToDisk(IDeviceStorage storage)
        {
            var path = CreateCurrentSessionStoragePath(storage);
            var serialized = JsonConvert.SerializeObject(_sessionEvents);
            storage.WriteText(path, serialized);
        }

        public void RestoreFromDisk(IDeviceStorage storage, DateTime savedDateTime)
        {
            var datePlusSessionTimeout = savedDateTime.AddSeconds(DefaultSessionTimeoutInSeconds);
            if (DateTime.Now.Ticks <= datePlusSessionTimeout.Ticks) return;

            var end = DateTime.UtcNow;
            var lenght = end.Subtract(new DateTime(_startDate));

            var path = CreateCurrentSessionStoragePath(storage);
            var content = storage.ReadAllText(path);

            var sessionEvent = new SessionEvent
            {
                sessionUUID = _currentSessionId,
                eventAttr = _eventAttributes,
                elapsedTime = lenght.TotalSeconds
                //StartDate = _startDate,
                //EndDate = end.Ticks,
            };

            var message = JsonConvert.DeserializeObject<List<Object>>(content);
            message.Add(sessionEvent);

            this.DoUpload(message);
        }

        private void DoUpload(IReadOnlyCollection<object> message)
        {
            this.DoUpload(JsonConvert.SerializeObject(message))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) return;
                    this.Reset();
                });
        }

        private string CreateCurrentSessionStoragePath(IDeviceStorage storage)
        {
            
            var folder = Path.Combine(storage.GetBasePath(), Subfolder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, _currentSessionId);
        }

        private void Reset()
        {
            this._currentSessionId = Guid.NewGuid().ToString();
            _sessionEvents = new List<Event>();
            if (_timerUploader.Enabled) return;

            _timerUploader.Enabled = true;
            _timerUploader.Start();
        }
    }
}