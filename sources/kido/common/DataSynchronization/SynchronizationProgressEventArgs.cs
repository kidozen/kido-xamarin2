using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
    public class ReplicationDetails<T>
    {
        public int NewCount { get; set; }
        public int RemoveCount { get; set; }
        public int UpdateCount { get; set; }
		public int ConflictCount { get; set; }

		public IEnumerable<T> News { get; set; }
		public IEnumerable<T> Deleted { get; set; }
		public IEnumerable<T> Updated { get; set; }
		public IEnumerable<T> Conflicts { get; set; }

	}

    public class SynchronizationProgressEventArgs : EventArgs
    {
        public SynchronizationType SynchronizationType { get; set; }
        public int CompletedChangesCount { get; set; }
        public int ChangesCount { get; set; }
    }

    public class SynchronizationCompleteEventArgs<T> : EventArgs
    {
        public SynchronizationType SynchronizationType { get; set; }
        public ReplicationDetails<T> Details { get; set; }
    }
}
