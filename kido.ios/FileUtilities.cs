
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

#if __UNIFIED__
using MonoTouch;
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Kidozen.iOS
{
	public class FileUtilities
	{
		public FileUtilities ()
		{
		}

        public static string getDocumentsFolder()
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
		public static void TestFastZipUnpack(string zipFileName, string targetDir) {
			FastZip fastZip = new FastZip();
			string fileFilter = null;
			// Will always overwrite if target filenames already exist
			fastZip.ExtractZip(zipFileName, targetDir, fileFilter);
		}

	}
}

