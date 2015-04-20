#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
    public class DataSyncDocument
    {
        internal string _id {get;set;}
        internal string _rev {get;set;}
    }
}
