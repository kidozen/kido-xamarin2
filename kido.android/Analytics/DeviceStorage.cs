using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kidozen.Android.Analytics

{
    class DeviceStorage : IDeviceStorage
    {
        public string GetBasePath()
        {
            return "";
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