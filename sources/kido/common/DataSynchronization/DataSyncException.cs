using System;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
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
