#if __IOS__
using Kidozen.iOS;

#else
using Kidozen.Android;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sync
{
    public class TodoItem : DataSyncDocument
    {
		public string id { get; set; }
		public string Name { get; set; }
        public string Notes { get; set; }
        public bool Done { get; set; }
    }

}
