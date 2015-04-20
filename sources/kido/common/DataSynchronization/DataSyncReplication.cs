using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if __ANDROID__
namespace Kidozen.Android
#else
using Foundation;
using UIKit;
namespace Kidozen.iOS
#endif
{
    class DataSyncReplication
    {
    }
}