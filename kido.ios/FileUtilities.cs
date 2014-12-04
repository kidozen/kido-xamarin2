using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;


namespace Kidozen.iOS
{
	public class FileUtilities
	{
		public FileUtilities ()
		{
		}

		public static void TestFastZipUnpack(string zipFileName, string targetDir) {
			FastZip fastZip = new FastZip();
			string fileFilter = null;
			// Will always overwrite if target filenames already exist
			fastZip.ExtractZip(zipFileName, targetDir, fileFilter);
		}

	}
}

