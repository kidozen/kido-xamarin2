#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
    public class DataSyncDocument
    {
		internal string _id {get;set;}
		internal string _rev {get;set;}
    }
}
