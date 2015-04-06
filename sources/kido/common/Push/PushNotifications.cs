using System;

#if __ANDROID__
using Android.Runtime;
namespace Kidozen.Android
    {
    [Preserve(AllMembers = true)]
#else
namespace Kidozen.iOS
    {
#endif
    public enum PushNotificationType { 
        Toast,
        Title,
        Raw
    }

    public class PushNotification
    {
        public PushNotificationType type { get; set; }
        public String title { get; set; }
        public String text { get; set; }
        public String image { get; set; }
        public int badge { get; set; }
    }

    internal class InternalPushNotification
    {
        public InternalPushNotification(PushNotification baseNotification)
        {
            this.type = Enum.GetName(typeof(PushNotificationType), baseNotification.type).ToLower();
            this.title = baseNotification.title;
            this.text = baseNotification.text;
            this.image = baseNotification.image;
            this.badge = baseNotification.badge;
        }

        public string type { get; set; }
        public String title { get; set; }
        public String text { get; set; }
        public String image { get; set; }
        public int badge { get; set; }
    }

}