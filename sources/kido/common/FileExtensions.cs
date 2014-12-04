using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.FSharp.Core;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using K = Kidozen;
using U = Utilities;
using A = Application;
using C = Crash;
using F = Files;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public static partial class FileExtensions
	{
		public static Task<MemoryStream> Download(this Files.Files files, string path) {
			var task = files.DownloadAsBytes(path).Result;
			return Task.Factory.StartNew<MemoryStream>(()=>{
				return new MemoryStream(task.Value);
			});		

		}
	}
}

/*
Sample usage:

app.Authenticate("loadtests@kidozen.com","pass","Kidozen")
	.ContinueWith(t=> {
		app.Files.Download("/abc/testsxamarinfile2.txt,testsxamarinfile2.txt")
			.ContinueWith(fd=>
				{
					var f = new System.IO.StreamReader(fd.Result).ReadToEndAsync().Result;
					System.Diagnostics.Debug.WriteLine(f);
				});
	});

*/