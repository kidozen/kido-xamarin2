using System;

namespace Kidozen.iOS
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
