using Android.Content;
using Android.Locations;
using Android.Net;
using Android.OS;
using Android.Telephony;
using Kidozen.Android;

namespace Kidozen.Android.Analytics
{
    public class DeviceInformation : IDeviceInformation
    {
        Context mContext;
        LocationHelper locationHelper = new LocationHelper();
        private Address mDefaultAddress = null;
        public DeviceInformation(Context context)
        {
            mDefaultAddress = locationHelper.GetAddress(context);
        }

        string GetIsoCountryCode()
        {
            return mDefaultAddress.CountryCode;
        }

        string GetNetworkAccess()
        {
            var manager = (ConnectivityManager)mContext.GetSystemService(Context.ConnectivityService);
            return manager == null ? "Unknown" : manager.ActiveNetworkInfo.ToString() ?? "Unknown";
        }

        string GetCarrierName()
        {
            var manager = (TelephonyManager) mContext.GetSystemService(Context.TelephonyService);
            return manager == null ? "Unknown" : manager.NetworkOperatorName ?? "Unknown";
        }

        string GetDeviceName()
        {
            var manufacturer = Build.Manufacturer;
            var model = Build.Model;
            if (model.StartsWith(manufacturer))
            {
                return model.ToUpper();
            }
            else
            {
                return manufacturer.ToUpper() + " " + model;
            }
        }

        public SessionAttributes GetAttributes()
        {
            return new SessionAttributes
            {
                isoCountryCode = GetIsoCountryCode(),
                platform = "Android",
                networkAccess = GetNetworkAccess(),
                carrierName = GetCarrierName(),
                systemVersion = Build.VERSION.Release,
                deviceModel = GetDeviceName()
            };
        }
    }
}