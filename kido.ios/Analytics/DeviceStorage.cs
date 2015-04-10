#if __UNIFIED__

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