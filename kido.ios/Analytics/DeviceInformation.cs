using CoreTelephony;
using UIKit;

namespace Kidozen.iOS.Analytics
{
    public class DeviceInformation : IDeviceInformation
    {
        public DeviceInformation()
        {
        }

        string GetIsoCountryCode()
        {
            using (var info = new CTCarrier())
            {
                return info.IsoCountryCode ?? "Unknown";
            }
        }

        string GetNetworkAccess()
        {
            return Reachability.InternetConnectionStatus().ToString();
        }


        string GetCarrierName()
        {
            using (var info = new CTTelephonyNetworkInfo())
            {
                var p = info.SubscriberCellularProvider;
                return p == null ? "Unknown" : p.CarrierName ?? "Unknown" ;
            }
        }

        public SessionAttributes GetAttributes()
        {
            return new SessionAttributes
            {
                isoCountryCode = GetIsoCountryCode(),
                platform = "iOS",
                networkAccess = GetNetworkAccess(),
                carrierName = GetCarrierName(),
                systemVersion = UIDevice.CurrentDevice.SystemVersion,
                deviceModel = UIDevice.CurrentDevice.Model
            };
        }
    }
}