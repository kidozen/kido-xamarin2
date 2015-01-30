using System.Linq;
using Android.Content;
using Android.Locations;
using Java.Lang;
using Java.Util;
using Exception = System.Exception;

namespace Kidozen.Analytics.Android
{
    class LocationHelper
    {
        public Address GetAddress(Context context)
        {
            var loc = GetLastBestLocation(context, 30);
            if (loc != null)
            {
                var gcd = new Geocoder(context, Locale.Default);
                var addresses = gcd.GetFromLocation(loc.Latitude, loc.Longitude, 1);
                return addresses.FirstOrDefault();
           }
            else
            {
                throw new Exception("Could not get location information. Did you enabled location services ?");
            }
        }
        public Location GetLastBestLocation(Context context, long minTime) 
        {
            Location bestResult = null;
            var bestAccuracy = float.MaxValue;
            var bestTime = Long.MinValue;
            var locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
            locationManager.AllProviders.ToList().ForEach(p =>
                {
                    var location = locationManager.GetLastKnownLocation(p);
                    if (location == null) return;
                    var accuracy = location.Accuracy;
                    var time = location.Time;
                    if (time>minTime && accuracy < bestAccuracy)
                    {
                        bestResult = location;
                        bestAccuracy = accuracy;
                        bestTime = time;
                    }
                    else if (time < minTime && bestAccuracy == float.MaxValue && time > bestTime)
                    {
                        bestResult = location;
                        bestTime = time;
                    }
                }
            );
        return bestResult;
        }
    }


}