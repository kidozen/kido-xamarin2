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
using A = KzApplication;
using C = Crash;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public static partial class ServiceExtensions
	{
		public static Task<Stream> Invoke<T>(this Service service, string method, T parameters) {
			var task = service.InvokeFile (method, parameters);
			return Task.Factory.StartNew<Stream>(()=>{
				return new MemoryStream(task.Result);
			});		
		}
	}
}
