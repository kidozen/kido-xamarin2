using System;
using System.IO;
using System.Diagnostics;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;


namespace Kidozen.iOS
{

	public class iOSDataVisualization : IDataVisualization
	{
		NSObject invoker = new NSObject();

		public iOSDataVisualization ()
		{
		}

		#region IDataVisualization implementation

		public void LaunchView (string visualization, string targetdirectory)
		{
			invoker.InvokeOnMainThread (() => {
				try {
					string filepath = Path.Combine(targetdirectory,visualization);
					filepath = Path.Combine(filepath,"index.html");
					var datavizController = new DataVisualizationViewController (filepath);
					var navController = new UINavigationController (datavizController);
					UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController (navController, 
						true, 
						new NSAction ( () => Debug.WriteLine("passive view loaded") )
					);
				} catch (Exception ex) {
					Debug.WriteLine (ex.Message);	
				}
			});

		}

		public string GetDestinationDirectory (string visualization)
		{
			var basepath = GetTargetDirectory();
			return Path.Combine (basepath, visualization);
		}

		public string GetTargetDirectory ()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
				var folders = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User);
				return folders[0].Path;
			} else {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				return Path.GetFullPath(Path.Combine (documents, "..", "Library","Caches"));
			}
		}

		public bool UnzipFiles (string path, string zipname)
		{
			try {
				using (ZipInputStream s = new ZipInputStream(System.IO.File.OpenRead(Path.Combine(path,zipname + ".zip")))) {
					ZipEntry theEntry;
					while ((theEntry = s.GetNextEntry()) != null) {

						Console.WriteLine(theEntry.Name);

						string directoryName = Path.GetDirectoryName(theEntry.Name);
						string fileName      = Path.GetFileName(theEntry.Name);
						string basepath = Path.Combine(path,zipname);
						// create directory
						if ( directoryName.Length > 0 ) {
							Directory.CreateDirectory(Path.Combine(basepath, directoryName));
						}

						if (fileName != String.Empty) {
							using (FileStream streamWriter = System.IO.File.Create(Path.Combine(basepath,theEntry.Name))) {

								int size = 2048;
								byte[] data = new byte[2048];
								while (true) {
									size = s.Read(data, 0, data.Length);
									if (size > 0) {
										streamWriter.Write(data, 0, size);
									} else {
										break;
									}
								}
							}
						}
					}
				}
				return true;
			} 
			catch (Exception ex) {
				Debug.WriteLine ("UnzipFiles: " + ex.Message);	
				return false;
			}
		}

		#endregion
	}
}

