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
    public enum SynchronizationType
    {
        Push,Pull,TwoWay
    }

	public enum PullConflictResolutionType {
		Default, ResolveWithRemote, ResolveWithLocal
	}
}
