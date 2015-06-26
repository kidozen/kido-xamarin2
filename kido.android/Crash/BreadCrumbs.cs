using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public class BreadCrumbs {
		private static List<string> _collection;
		private List<string> collection { 
			get {
				if (_collection==null) _collection = new List<string>();
				return _collection;
			}
		}

		public void Add( string Value ) {
			this.collection.Add(Value);
		}

		public List<string> GetAll() { return this.collection; }
	}
}