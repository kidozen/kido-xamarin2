using Kidozen.Android;
namespace Kidozen.Android.Analytics
{
    public class DeviceInformation : IDeviceInformation
    {
        public DeviceInformation()
        {
        }

        string GetIsoCountryCode()
        {
            return "Unknown";
        }

        string GetNetworkAccess()
        {
            return "Unknown";
        }


        string GetCarrierName()
        {
            return "Unknown";
        }

        public SessionAttributes GetAttributes()
        {
            return new SessionAttributes
            {
                isoCountryCode = GetIsoCountryCode(),
                platform = "Android",
                networkAccess = GetNetworkAccess(),
                carrierName = GetCarrierName(),
                systemVersion = "Unknown",
                deviceModel = "Unknown"
            };
        }
    }
}