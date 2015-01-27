using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if __ANDROID__
using Android.Runtime;
namespace Kidozen.Android
    {
#else
namespace Kidozen.iOS
    {
#endif
    public class Event
    {
        public Event() { }
        public string eventName { get; set; }
    }

    public class ValueEvent : Event
    {
        public ValueEvent() {}
        public object eventValue { get; set; }
        public string sessionUUID { get; set; }
    }

    public class SessionEvent : Event
    {
        public SessionEvent() {}
        public SessionAttributes eventAttr { get; set; }
        public string sessionUUID { get; set; }
        public int length { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string eventValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long EndDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long StartDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool isPending { get; set; }
    }


    public class SessionAttributes
    {
        public SessionAttributes() { }
        public string isoCountryCode { get; set; }
        public string platform { get; set; }
        public string sessionLength { get; set; }
        public string networkAccess { get; set; }
        public string carrierName { get; set; }
        public string systemVersion { get; set; }
        public string deviceModel { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TAG { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string adminArea { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string countryName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string locale { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string locality { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string subAdminArea { get; set; }

    }

}