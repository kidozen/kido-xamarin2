using System;
using System.IO;
using Android.Content;
using Java.IO;
using Java.Util.Zip;
using A = KzApplication;
using C = Crash;

using AOS = Android.OS;
using Console = System.Console;
using IOException = Java.IO.IOException;

namespace Kidozen.Android
{
	public class AndroidDataVisualization : IDataVisualization
	{
		public Context Context { get; set;}
		public AndroidDataVisualization ()
		{
		}

		#region IDataVisualization implementation

		public void LaunchView (string visualization, string targetdirectory)
		{
			var startPassiveAuthIntent = new Intent (this.Context, typeof(DataVisualizationActivity));
			startPassiveAuthIntent.AddFlags (ActivityFlags.NewTask);
			startPassiveAuthIntent.PutExtra ("visualization", visualization);
			startPassiveAuthIntent.PutExtra ("destinationdir", targetdirectory);
			Context.StartActivity (startPassiveAuthIntent);
		}

		public string GetDestinationDirectory (string visualization)
		{
			return AOS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + visualization;
		}

		public string GetTargetDirectory ()
		{
			return AOS.Environment.ExternalStorageDirectory.AbsolutePath.ToString ();
		}

		public bool UnzipFiles (string path, string zipname)
		{
			var f = new Java.IO.File(path);
			if (!f.Exists()) f.Mkdir();
			try
			{
				String filename;
				var inputstream = System.IO.File.Open(Path.Combine(path,zipname + ".zip"),FileMode.Open);
				var zis = new ZipInputStream(inputstream);
				ZipEntry ze;
				byte[] buffer = new byte[1024];
				int count;

				while ((ze = zis.NextEntry) != null)
				{
					filename = ze.Name;
					// Need to create directories if not exists, or
					// it will generate an Exception...
					if (ze.IsDirectory) {
						Java.IO.File fmd = new Java.IO.File( Path.Combine(Path.Combine(path,zipname ) ,filename));
						fmd.Mkdirs();
						continue;
					}

					var fout = new FileOutputStream(Path.Combine(Path.Combine(path,zipname ) ,filename));
					while ((count = zis.Read(buffer)) != -1) fout.Write(buffer, 0, count);

					fout.Close();
					zis.CloseEntry();
				}

				zis.Close();
			}
			catch(IOException iox)
			{
				Console.WriteLine (iox.Message);
				return false;
			}

			return true;
		}

		#endregion
	}
}

