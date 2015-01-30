using System;
using AOS = Android.OS;

namespace Kidozen.Analytics.Android

{
    class DeviceStorage : IDeviceStorage
    {
        public string GetBasePath()
        {
            return AOS.Environment.ExternalStorageDirectory.AbsolutePath.ToString();
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