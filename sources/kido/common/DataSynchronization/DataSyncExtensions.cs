using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
    public static class DataSynchronizationExtensions
    {
        public static DataSync<T> DataSync<T>(this KidoApplication app, string name)
        {
            return new DataSync<T>(name, app);
        }
    }
}