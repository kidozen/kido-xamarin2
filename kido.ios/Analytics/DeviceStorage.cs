using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if __UNIFIED__
using MonoTouch;
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Kidozen.iOS.Analytics
{
    class DeviceStorage : IDeviceStorage
    {
        public string GetBasePath()
        {
            return FileUtilities.GetDocumentsFolder();
        }

        public void WriteText(string path, string content)
        {
            System.IO.File.WriteAllText(path, content);
        }

        public string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}