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
    public class ReplicationDetails
    {
        public int News { get; set; }
        public int Deleted { get; set; }
        public int Updated { get; set; }
		public int Conflicts { get; set; }
    }

    public class SynchronizationProgressEventArgs : EventArgs
    {
        public SynchronizationType SynchronizationType { get; set; }
        public int CompletedChangesCount { get; set; }
        public int ChangesCount { get; set; }
    }

    public class SynchronizationCompleteEventArgs : EventArgs
    {
        public SynchronizationType SynchronizationType { get; set; }
        public ReplicationDetails Details { get; set; }
    }
}
