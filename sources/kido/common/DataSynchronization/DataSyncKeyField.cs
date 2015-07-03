using System;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
    class DataSyncKeyFieldAttribute : Attribute
    {
        public int order = 0;
        public DataSyncKeyFieldAttribute() {}
        public DataSyncKeyFieldAttribute(int order)
        {
            this.order = order;
        }
    }
}
