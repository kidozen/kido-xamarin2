using System;
using System.IO;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif


namespace Kidozen.iOS
{
	public class FileUtilities
	{
		public FileUtilities ()
		{
		}

        public static string GetDocumentsFolder()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var folders = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User);
                return folders[0].Path;
            }
            else
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.GetFullPath(Path.Combine(documents, "..", "Library", "Caches"));
            }
        }

        public static String GetNsUserDefaultStoredValue(string key)
        {
            var value = NSUserDefaults.StandardUserDefaults.StringForKey(key);
            return value ?? string.Empty;
        }

        public static void SetNsUserDefaultStoredValue(string key, string value)
        {
            NSUserDefaults.StandardUserDefaults.SetString(value, key);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public static void DeleteNsUserDefaultStoredValue(string key)
        {
            NSUserDefaults.StandardUserDefaults.RemoveObject(key);
        }
	}
}

