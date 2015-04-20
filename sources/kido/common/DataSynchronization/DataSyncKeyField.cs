using System;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
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
