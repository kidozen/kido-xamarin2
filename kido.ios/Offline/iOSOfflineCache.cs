using System;
using System.IO;
using System.Diagnostics;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Kidozen.iOS.Offline
{
	public class iOSOfflineCache : IOfflineCache
	{
		#region IOfflineCache implementation

		public string GetTargetDirectory ()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
				var folders = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User);
				return folders[0].Path;
			} else {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				return Path.GetFullPath(Path.Combine (documents, "..", "Library","Application Support"));
			}
		}

		#endregion

		public iOSOfflineCache ()
		{
		}
	}
}

