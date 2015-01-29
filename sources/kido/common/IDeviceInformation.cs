#if __ANDROID__
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
