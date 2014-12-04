using System;
using AOS = Android.OS ;

namespace Kidozen.Android
{
	public class AndroidOfflineCache: IOfflineCache
	{
		#region IOfflineCache implementation

		public string GetTargetDirectory ()
		{
			return AOS.Environment.ExternalStorageDirectory.AbsolutePath.ToString ();
		}

		#endregion

		public AndroidOfflineCache ()
		{
		}
	}
}

