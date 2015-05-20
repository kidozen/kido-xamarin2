using System;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
    class DataSyncException : Exception
    {
        private string p;

        public DataSyncException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}
