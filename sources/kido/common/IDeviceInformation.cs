#if __ANDROID__
using Android.Runtime;
namespace Kidozen.Android
    {
#else
namespace Kidozen.iOS
    {
#endif
    public interface IDeviceInformation
    {
        SessionAttributes GetAttributes();
    }
}
