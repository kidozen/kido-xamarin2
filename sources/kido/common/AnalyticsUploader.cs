using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

#if __ANDROID__
using Android.Runtime;
namespace Kidozen.Android
    {
#else
namespace Kidozen.iOS
{
#endif
    public class AnalyticsUploader
    {
    }
}