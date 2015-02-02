using Newtonsoft.Json;

namespace Kidozen.Analytics 
{
    public class Event
    {
        public Event() { }
        public virtual string eventName { get; set; }
    }

    public class ValueEvent : Event
    {
        public ValueEvent() {}
        public override string eventName
        {
            get
            {
                return base.eventName;
            }
            set
            {
                base.eventName = value;
            }
        }
        public object eventValue { get; set; }
        public string sessionUUID { get; set; }
    }

    public class CustomEvent<T> : Event
    {
        public CustomEvent() { }
        public override string eventName
        {
            get
            {
                return base.eventName;
            }
            set
            {
                base.eventName = value;
            }
        }
        public T eventAttr { get; set; }
        public string sessionUUID { get; set; }
    }

    public class SessionEvent : Event
    {
        const string EVENT_NAME = "usersession";
        public SessionEvent() {}
        public override string eventName
        {
            get
            {
                return EVENT_NAME;
            }
            set
            {
                base.eventName = EVENT_NAME;
            }
        }

        public string sessionUUID { get; set; }
        public double elapsedTime { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string eventValue { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public long EndDate { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public long StartDate { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool isPending { get; set; }
        public SessionAttributes eventAttr { get; set; }
    }

    public class SessionAttributes
    {
        public SessionAttributes() { }
        public string isoCountryCode { get; set; }
        public string platform { get; set; }
        public string networkAccess { get; set; }
        public string carrierName { get; set; }
        public string systemVersion { get; set; }
        public string deviceModel { get; set; }
    }

}