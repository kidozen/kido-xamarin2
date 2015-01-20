using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if __ANDROID__
using Android.Runtime;
namespace Kidozen.Android
    {
    [Preserve(AllMembers = true)]
#else
namespace Kidozen.iOS
    {
#endif
	public static partial class FileExtensions
	{
		public static Task<MemoryStream> Download(this Files files, string path) {
			var task = files.DownloadAsBytes(path).Result;
			return Task.Factory.StartNew<MemoryStream>(()=>{
				return new MemoryStream(task);
			});		

		}
	}
}