﻿using System.IO;
using System.Threading.Tasks;
using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public static partial class NativeUtilities
	{
		public static Task<MemoryStream> Download(this Files files, string path) {
			var task = files.DownloadAsBytes(path).Result;
			return Task.Factory.StartNew<MemoryStream>(()=>{
				return new MemoryStream(task);
			});		

		}
	}
}
