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
	public static partial class DataSourcesExtensions
	{
		public static Task<Stream> Query(this DataSource ds) {
			var task = ds.QueryFile();
			return Task.Factory.StartNew<Stream>(()=>{
				return new MemoryStream(task.Result);
			});		
		}

		public static Task<Stream> Invoke(this DataSource ds) {
			var task = ds.InvokeFile();
			return Task.Factory.StartNew<Stream>(()=>{
				return new MemoryStream(task.Result);
			});		
		}

		public static Task<Stream> Query<T>(this DataSource ds, T parameters) {
			var task = ds.QueryFile(parameters);
			return Task.Factory.StartNew<Stream>(()=>{
				return new MemoryStream(task.Result);
			});		
		}

		public static Task<Stream> Invoke<T>(this DataSource ds, T parameters) {
			var task = ds.InvokeFile(parameters);
			return Task.Factory.StartNew<Stream>(()=>{
				return new MemoryStream(task.Result);
			});		
		}
	}
}
