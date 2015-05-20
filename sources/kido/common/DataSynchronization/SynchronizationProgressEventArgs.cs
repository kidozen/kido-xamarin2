using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
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
