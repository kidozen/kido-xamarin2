
#if __ANDROID__
namespace Kidozen.Android.Offline
#else
namespace Kidozen.iOS.Offline
#endif
{
	public enum OfflineCacheEnumeration
	{
		NetworkOnly,
		LocalElseNetwork,
		NetworkElseLocal
	}
}

